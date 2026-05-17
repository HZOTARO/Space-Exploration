using System.Collections.Generic;

public static class SyntaxDictionary
{
    public static readonly string[] Core = {
        "Module", "Expr", "Call", "Name", "Load", "Store", "Constant", "Pass", "keyword", "arg",
        "JoinedStr",        // f-strings
        "FormattedValue",   // f-string values

        "Add", "Sub", "Mult", "Div", "FloorDiv", "Mod", "Pow",
        "BinOp",    // Any math operation
        "UnaryOp",  // Needed for negative numbers (-5)
        "USub"      // '-' sign for negative numbers
    };

    public static readonly string[] Variables = {
        "Assign",
        "AugAssign" // +=
    };

    public static readonly string[] Logic = {
        "If", "IfExp",

        "Match", "match_case", "MatchValue", "MatchOr",
        "MatchAs",        // case _:
        "MatchSingleton", // case None:
        "MatchSequence",  // case [1, 2, 3]:

        "Compare", "Eq", "NotEq", "Lt", "LtE", "Gt", "GtE", 
        
        
        "BoolOp", "And", "Or", "Not",

        "Is", "IsNot"
    };

    public static readonly string[] Loops = {
        "For", "While", "Break", "Continue"
    };

    public static readonly string[] Lists = {
        "List",
        "Subscript", // my_list[0]
        "Slice",     // my_list[0:2]
        "Attribute", // allow for methods like .append()

        "Delete", "Del",

        "In", "NotIn",

        "Starred"    // [1, *other] combine
    };
}

public static class FunctionDictionary
{
    public static readonly string[] Core = {
        "print", "range", "len", "int", "float", "str", "bool", "type", "abs", "max", "min", "sum", "round", "list"
    };
}

public static class ErrorDictionary
{
    public static readonly Dictionary<string, string> ErrorTranslations = new Dictionary<string, string>()
    {
        { "Assign", "Variables are not unlocked yet!" },
        { "AugAssign", "Variables are not unlocked yet!" },
        { "If", "If/Else statements are not unlocked yet!" },
        { "For", "Loops are not unlocked yet!" },
        { "While", "Loops are not unlocked yet!" },
        { "List", "Lists are not unlocked yet!" }
    };
}