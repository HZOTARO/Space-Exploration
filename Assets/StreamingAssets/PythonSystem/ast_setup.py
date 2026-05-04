import ast
import types
import json

# |=================|
# | SYNTAX LIMITING |
# |=================|

current_banned_nodes = []
current_banned_functions = []

def initialize_bans(json_string):
    global current_banned_nodes, current_banned_functions
    data = json.loads(json_string)
    current_banned_nodes = data.get("banned_nodes", [])
    current_banned_functions = data.get("banned_functions", [])
    return "Bans Initialized"

def clear_ban(ban_name):
    global current_banned_nodes, current_banned_functions
    if ban_name in current_banned_nodes:
        current_banned_nodes.remove(ban_name)
    if ban_name in current_banned_functions:
        current_banned_functions.remove(ban_name)
    return "Cleared"

def validate_code(source_code):
    try:
        tree = ast.parse(source_code)
    except SyntaxError as e:
        return json.dumps({"is_valid": False, "error_msg": f"Syntax Error: {e.msg}", "line": e.lineno})

    for node in ast.walk(tree):
        node_type = type(node).__name__ 
        if node_type in current_banned_nodes:
            return json.dumps({"is_valid": False, "error_msg": f"'{node_type}' is locked!", "line": getattr(node, 'lineno', 1)})
            
        if isinstance(node, ast.Call) and isinstance(node.func, ast.Name):
            if node.func.id in current_banned_functions:
                return json.dumps({"is_valid": False, "error_msg": f"'{node.func.id}()' is locked!", "line": getattr(node, 'lineno', 1)})

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
        self.global_names.add(node.name)
        
    def visit_ClassDef(self, node):
        self.global_names.add(node.name)
        
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
        return self.insert_yield(node)

    def visit_For(self, node):
        self.generic_visit(node)
        node.body.insert(0, self.create_yield_node(node))
        return self.insert_yield(node)

    def visit_While(self, node):
        self.generic_visit(node)
        node.body.insert(0, self.create_yield_node(node))
        return self.insert_yield(node)

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

__gen__ = None

# |================|
# | STEP EXECUTION |
# |================|
def prepare(code):
    global __gen__
    
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

        __gen__ = env['__runner__']()
        
    except Exception as e:
        print(f'{type(e).__name__}: {e}')
        __gen__ = None

def step():
    global __gen__
    
    if __gen__ is None:
        return 'ERROR'

    try:
        yielded_value = next(__gen__)
        if yielded_value is not None:
            return str(yielded_value)

        return 'STEP'
    except StopIteration:
        return 'DONE'
    except Exception as e:
        print(f'{type(e).__name__}: {e}')
        return 'ERROR'