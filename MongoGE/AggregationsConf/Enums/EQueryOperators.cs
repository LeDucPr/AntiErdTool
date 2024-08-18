namespace MongoGE.AggregationsConf.Enums
{
    enum EComparisonOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
    enum ELogicalOperator
    {
        And,
        Or,
        Not
    }
    enum ESetOperator
    {
        In,
        NotIn,
        All,
        Size,
        Exists
    }
    enum EElementOperator
    {
        Regex,
        Type,
    }
    enum StageOperator
    {
        Match,
        Project,
        Group,
        Sort,
        Skip,
        Limit,
        Unwind,
        Lookup,
        AddFields,
        ReplaceRoot,
        Merge,
        Bucket,
        Sample,
        Count,
        Facet,
        ConcatArrays
    }
}
