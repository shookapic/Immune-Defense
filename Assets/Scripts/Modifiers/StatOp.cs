// Operation applied by a modifier: either additive (+X) or multiplicative (*X).

namespace ImmuneDefense.Modifiers
{
    public enum StatOp
    {
        Add = 0,       // +X
        Multiply = 1,  // *X (e.g., 1.10 for +10%)
    }
}
