# AntiErdTool
 Generic constructure can be apply for many erd
 (Sử dụng theo cấu trúc của MongoDB)
 DataLayer : Client -+- DB_Ctrl -+- Collection_Ctrl -+- Doc 
                     |           |                   |  
                     + ...       + ...               + ...
 
 Object    : BasicComponent : BsonValue (Tree Doc) 
             EntityComponent : BasicComponent (Object for interacter)
             PrimeComponent : EntityComponent (Data for endpoint loader)
             PrimitiveDataComponent : PrimeComponent (Endpoint loader)
 OverallUV : Map to BasicComponent (Objects Tree Mapper ) -> DataLayer 
 PipelineConnector : Map to OverallUV  (Data Transfer) 
 
 Aggregation:  OperatorHelper -> (aggregate operator / find)
