namespace MongoGE.AggregationsConf.Enums
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class OperatorAttribute : Attribute
    {
        public string Operator { get; }
        public OperatorAttribute(string operatorName)
        {
            Operator = operatorName;
        }
    }
    public enum EComparisonOperator
    {
        [Operator("$eq")]
        Equal,
        [Operator("$ne")]
        NotEqual,
        [Operator("$gt")]
        GreaterThan,
        [Operator("$gte")]
        GreaterThanOrEqual,
        [Operator("$lt")]
        LessThan,
        [Operator("$lte")]
        LessThanOrEqual
    }
    public enum ELogicalOperator
    {
        [Operator("$and")]
        And,
        [Operator("$or")]
        Or,
        [Operator("$not")]
        Not
    }
    public enum ESetOperator
    {
        [Operator("$in")]
        In,
        [Operator("$nin")]
        NotIn,
        [Operator("$all")]
        All,
        [Operator("$size")]
        Size,
        [Operator("$exists")]
        Exists
    }
    public enum EElementOperator
    {
        [Operator("$regex")]
        Regex,
        [Operator("$type")]
        Type,
    }
    public enum EStageOperator
    {
        [Operator("$match")]
        Match,
        [Operator("$project")]
        Project,
        [Operator("$group")]
        Group,
        [Operator("$sort")]
        Sort,
        [Operator("$skip")]
        Skip,
        [Operator("$limit")]
        Limit,
        [Operator("$unwind")]
        Unwind,
        [Operator("$lookup")]
        Lookup,
        [Operator("$addFields")]
        AddFields,
        [Operator("$replaceRoot")]
        ReplaceRoot,
        [Operator("$merge")]
        Merge,
        [Operator("$bucket")]
        Bucket,
        [Operator("$sample")]
        Sample,
        [Operator("$count")]
        Count,
        [Operator("$facet")]
        Facet,
        [Operator("$concatArrays")]
        ConcatArrays
    }
}
