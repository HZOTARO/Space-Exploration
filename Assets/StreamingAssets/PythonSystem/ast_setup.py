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
    global __gen__, __init_error__
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