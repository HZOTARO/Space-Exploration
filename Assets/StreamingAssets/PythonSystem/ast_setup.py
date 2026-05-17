import ast
import types
import json
import sys
import traceback

# |=================|
# | SYNTAX LIMITING |
# |=================|

current_banned_nodes = []
current_banned_functions = []

def initialize_allowed(json_string):
    global current_allowed_nodes, current_allowed_functions
    data = json.loads(json_string)
    current_allowed_nodes = data.get("allowed_nodes", [])
    current_allowed_functions = data.get("allowed_functions", [])
    return "Allowed Lists Initialized"

def unlock_syntax(unlock_name):
    global current_allowed_nodes, current_allowed_functions
    if unlock_name not in current_allowed_nodes:
        current_allowed_nodes.append(unlock_name)
    if unlock_name not in current_allowed_functions:
        current_allowed_functions.append(unlock_name)
    return "Unlocked"

def validate_code(source_code):
    try:
        tree = ast.parse(source_code)
    except SyntaxError as e:
        return json.dumps({"is_valid": False, "error_msg": f"Syntax Error: {e.msg}", "line": e.lineno})

    parent_map = {child: node for node in ast.walk(tree) for child in ast.iter_child_nodes(node)}
    
    for node in ast.walk(tree):
        node_type = type(node).__name__ 
        
        line_no = getattr(node, 'lineno', None)

        if line_no is None:
            curr = node
            while curr in parent_map:
                curr = parent_map[curr]
                line_no = getattr(curr, 'lineno', None)
                if line_no is not None:
                    break

            if line_no is None:
                line_no = 1

        if node_type not in current_allowed_nodes:
            return json.dumps({
                "is_valid": False, 
                "error_msg": f"Syntax '{node_type}' is locked or not allowed!", 
                "line": line_no
            })
            
        if isinstance(node, ast.Call) and isinstance(node.func, ast.Name):
            func_name = node.func.id
            if func_name not in current_allowed_functions:
                return json.dumps({
                    "is_valid": False, 
                    "error_msg": f"Function '{func_name}()' is locked or not allowed!", 
                    "line": line_no
                })

    return json.dumps({"is_valid": True, "error_msg": "Success", "line": -1})

# |========================|
# | STEP EXECUTION SETTING |
# |========================|

class GlobalCollector(ast.NodeVisitor):
    def __init__(self):
        self.global_names = set()
        
    def visit_Name(self, node):
        if isinstance(node.ctx, ast.Store):
            self.global_names.add(node.id)
            
    def visit_FunctionDef(self, node):
        self.generic_visit(node)
        
        return [
            self.create_yield_node(node), 
            node
        ]
        
    def visit_ClassDef(self, node):
        self.generic_visit(node)
        
        return [
            self.create_yield_node(node), 
            node
        ]
        
    def visit_Import(self, node):
        for alias in node.names:
            name = alias.asname or alias.name
            self.global_names.add(name.split('.')[0])
            
    def visit_ImportFrom(self, node):
        for alias in node.names:
            self.global_names.add(alias.asname or alias.name)

class YieldInserter(ast.NodeTransformer):
    def insert_yield(self, node):
        start_line = getattr(node, 'lineno', 0)
        end_line = getattr(node, 'end_lineno', start_line)
        val = f"{start_line},{end_line}"

        return [
            node,
            ast.Expr(value=ast.Yield(value=ast.Constant(value=val)))
        ]

    def create_yield_node(self, node):
        start_line = getattr(node, 'lineno', 0)
        end_line = getattr(node, 'end_lineno', start_line)
        val = f"{start_line},{end_line}"
        return ast.Expr(value=ast.Yield(value=ast.Constant(value=val)))

    def visit_Expr(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_Assign(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_AugAssign(self, node):
        self.generic_visit(node)
        return self.insert_yield(node)

    def visit_Return(self, node):
        self.generic_visit(node)
        start_line = getattr(node, 'lineno', 0)
        end_line = getattr(node, 'end_lineno', start_line)
        val = f"{start_line},{end_line}"
        return [
            ast.Expr(value=ast.Yield(value=ast.Constant(value=val))),
            node
        ]

    def visit_If(self, node):
        self.generic_visit(node)
        yield_node = self.create_yield_node(node)
        return [
            yield_node, 
            node
        ]

    def visit_For(self, node):
        self.generic_visit(node)
        node.body.insert(0, self.create_yield_node(node))
        
        return [
            self.create_yield_node(node), 
            node
        ]

    def visit_While(self, node):
        self.generic_visit(node)
        node.body.insert(0, self.create_yield_node(node))
        
        return [
            self.create_yield_node(node), 
            node
        ]

    def visit_Call(self, node):
        self.generic_visit(node)
        
        wrapper = ast.Name(id='__wrap_call__', ctx=ast.Load())
        new_args = [node.func] + node.args
        new_call = ast.Call(func=wrapper, args=new_args, keywords=node.keywords)
        
        return ast.YieldFrom(value=new_call)

def __wrap_call__(func, *args, **kwargs):
    res = func(*args, **kwargs)
    
    if isinstance(res, types.GeneratorType):
        return (yield from res)
        
    return res

# |================|
# | STEP EXECUTION |
# |================|

__gen__ = None
__init_error__ = None
__player_env__ = None

def prepare(code):
    global __gen__, __init_error__, __player_env__
    __init_error__ = None
    
    try:
        tree = ast.parse(code)

        collector = GlobalCollector()
        collector.visit(tree)

        transformer = YieldInserter()
        tree = transformer.visit(tree)

        body = tree.body

        if collector.global_names:
            body.insert(0, ast.Global(names=list(collector.global_names)))

        func_def = ast.FunctionDef(
            name='__runner__',
            args=ast.arguments(
                posonlyargs=[], args=[], kwonlyargs=[],
                kw_defaults=[], defaults=[]
            ),
            body=body,
            decorator_list=[]
        )

        module = ast.Module(body=[func_def], type_ignores=[])
        ast.fix_missing_locations(module)

        compiled = compile(module, '<player_code>', 'exec')

        env = globals().copy() 
        env['__wrap_call__'] = __wrap_call__
        exec(compiled, env)

        __player_env__ = env
        __gen__ = env['__runner__']()
        
    except Exception as e:
        exc_type, exc_value, exc_tb = sys.exc_info()
        line_no = 1
        for frame in traceback.extract_tb(exc_tb):
            if frame.filename == '<player_code>':
                line_no = frame.lineno
                break
                
        __init_error__ = f"RUNTIME_ERROR|{line_no}|{type(e).__name__}: {e}"
        __gen__ = None

def step():
    global __gen__, __init_error__
    
    if __init_error__ is not None:
        err = __init_error__
        __init_error__ = None
        return err

    if __gen__ is None:
        return 'DONE'

    try:
        yielded_value = next(__gen__)
        if yielded_value is not None:
            return str(yielded_value)

        return 'STEP'
    except StopIteration:
        return 'DONE'
    except Exception as e:
        exc_type, exc_value, exc_tb = sys.exc_info()
        line_no = 1
        for frame in traceback.extract_tb(exc_tb):
            if frame.filename == '<player_code>':
                line_no = frame.lineno
                break
                
        return f"RUNTIME_ERROR|{line_no}|{type(e).__name__}: {e}"

def get_variable_value(var_name):
    global __gen__, __player_env__
    
    if __gen__ is not None and hasattr(__gen__, 'gi_frame') and __gen__.gi_frame is not None:
        local_vars = __gen__.gi_frame.f_locals
        if var_name in local_vars:
            return str(local_vars[var_name])
            
    if __player_env__ is not None and var_name in __player_env__:
        return str(__player_env__[var_name])
        
    return "Undefined"

def get_line_syntax_map(source_code):
    try:
        tree = ast.parse(source_code)
        line_map = {}
        
        for node in ast.walk(tree):
            if hasattr(node, 'lineno'):
                line = node.lineno
                node_type = type(node).__name__
                
                if line not in line_map:
                    line_map[line] = set()
                line_map[line].add(node_type)
        
        result = []
        for line, nodes in line_map.items():
            result.append(f"{line}:{','.join(nodes)}")
            
        return "|".join(result)
    except Exception:
        return ""

def check_ast_pattern(source_code, start_line, end_line, pattern, target):
    try:
        tree = ast.parse(source_code)

        def contains_call(body_list, target_func):
            for stmt in body_list:
                for child in ast.walk(stmt):
                    if isinstance(child, ast.Call):
                        if isinstance(child.func, ast.Name) and child.func.id == target_func:
                            return True
                        if isinstance(child.func, ast.Attribute) and isinstance(child.func.value, ast.Name):
                            full_name = f"{child.func.value.id}.{child.func.attr}"
                            if full_name == target_func:
                                return True
            return False
            
        def contains_nested_for(body_list):
            for stmt in body_list:
                for child in ast.walk(stmt):
                    if isinstance(child, ast.For): return True
            return False
                            
        for node in ast.walk(tree):
            line = getattr(node, 'lineno', -1)
            if int(start_line) <= line <= int(end_line):

                if pattern == "FunctionCall":
                    if isinstance(node, ast.Call):
                        if int(start_line) <= line <= int(end_line):
                            if isinstance(node.func, ast.Name) and node.func.id == target:
                                return "True"

                elif pattern == "ScanAndPrintVar":
                    scan_variables = set()
                    printed_var = None
                    
                    sorted_nodes = sorted(
                        [n for n in ast.walk(tree) if hasattr(n, 'lineno')], 
                        key=lambda x: x.lineno
                    )
                    
                    for n in sorted_nodes:
                        if isinstance(n, ast.Assign):
                            for target in n.targets:
                                if isinstance(target, ast.Name):
                                    var_name = target.id
                                    
                                    if isinstance(n.value, ast.Call) and getattr(n.value.func, 'id', '') == "scan":
                                        scan_variables.add(var_name)
                                    elif isinstance(n.value, ast.Name) and n.value.id in scan_variables:
                                        scan_variables.add(var_name)
                                    else:
                                        if var_name in scan_variables:
                                            scan_variables.remove(var_name)
                                            
                        if isinstance(n, ast.Call) and getattr(n.func, 'id', '') == "print":
                            if len(n.args) > 0 and isinstance(n.args[0], ast.Name):
                                printed_var = n.args[0].id

                    if scan_variables and printed_var and (printed_var in scan_variables):
                        return "True"

                elif pattern == "FuncInsideIfWhiteOre":
                    scan_variables = set()
                    for n in ast.walk(tree):
                        if isinstance(n, ast.Assign) and isinstance(n.value, ast.Call) and getattr(n.value.func, 'id', '') == "scan":
                            for target_node in n.targets:
                                if isinstance(target_node, ast.Name):
                                    scan_variables.add(target_node.id)

                    for node in ast.walk(tree):
                        if isinstance(node, ast.If):
                            is_valid_conditional = False
                            
                            if isinstance(node.test, ast.Compare) and len(node.test.ops) == 1 and isinstance(node.test.ops[0], ast.Eq):
                                left = node.test.left
                                right = node.test.comparators[0]
                                
                                has_white_ore_literal = False
                                comparison_target = None
                                
                                if (isinstance(left, ast.Constant) and left.value == "WhiteOre") or (isinstance(left, ast.Str) and left.s == "WhiteOre"):
                                    has_white_ore_literal = True
                                    comparison_target = right
                                elif (isinstance(right, ast.Constant) and right.value == "WhiteOre") or (isinstance(right, ast.Str) and right.s == "WhiteOre"):
                                    has_white_ore_literal = True
                                    comparison_target = left
                                    
                                if has_white_ore_literal:
                                    if isinstance(comparison_target, ast.Call) and getattr(comparison_target.func, 'id', '') == "scan":
                                        is_valid_conditional = True
                                    elif isinstance(comparison_target, ast.Name) and comparison_target.id in scan_variables:
                                        is_valid_conditional = True

                            if is_valid_conditional and contains_call(node.body, target):
                                return "True"

                elif pattern == "FuncInsideFor":
                    if isinstance(node, ast.For) and contains_call(node.body, target): return "True"

                elif pattern == "FuncInsideWhile":
                    if isinstance(node, ast.While) and contains_call(node.body, target): return "True"

                elif pattern == "NestedFor":
                    if isinstance(node, ast.For) and contains_nested_for(node.body): return "True"

                elif pattern == "AssignList":
                    if isinstance(node, ast.Assign):
                        for t in node.targets:
                            if isinstance(t, ast.Name) and t.id == target:
                                if isinstance(node.value, ast.List): return "True"
                                if isinstance(node.value, ast.Call) and getattr(node.value.func, 'id', '') == "list": return "True"
                                
        return "False"
    except Exception as e:
        return "False"