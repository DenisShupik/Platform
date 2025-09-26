### New Rules

 Rule ID | Category     | Severity | Notes                                                                                                                                                                                      
---------|--------------|----------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
 GP0000  | Generator    | Error    | Source generator internal error. `An unexpected exception occurred inside the source generator: {0}`
 GP0001  | GenerateBind | Error    | Property must have binding attribute. `Property '{0}' must have one of [FromRoute], [FromQuery], [FromBody]`                                                                               
 GP0002  | GenerateBind | Error    | Property must not have multiple binding attributes. `Property '{0}' must have exactly one binding attribute among [FromRoute], [FromQuery], [FromBody]`                                    
 GP0003  | GenerateBind | Error    | Nullable parameter must not define a default value. `Parameter '{0}' is nullable and must not specify a default value`                                                                     
 GP0004  | GenerateBind | Error    | Property marked with \[FromRoute] must be non-nullable. `Property '{0}' is marked with [FromRoute] and must be non-nullable`                                                               
 GP0005  | GenerateBind | Error    | Member marked with \[FromRoute] must not define a default value. `Member '{0}' is marked with [FromRoute] and must not specify a default value`                                            
 GP0006  | GenerateBind | Error    | Defaults contains member with no matching property. `Defaults contains member '{0}' but no matching public property '{0}' exists in the enclosing [GenerateBind] type`                     
 GP0007  | GenerateBind | Error    | Initializer not allowed in \[GenerateBind] class. `Property '{0}' must not have an initializer in a type annotated with [GenerateBind]. Move default values to the nested Defaults class.` 
 GP0008  | GenerateBind | Error    | Property must be declared 'required'. `Property '{0}' must be declared with the 'required' modifier in a [GenerateBind] type`                                                              
 GP0009  | GenerateBind | Error    | Property must have 'get; init;' accessors. `Property '{0}' must declare accessors 'get; init;' (auto-property) in a [GenerateBind] type`                                                   
 GP0010  | GenerateBind | Error    | Only one \[FromBody] is allowed. `Only one property may be annotated with [FromBody] in a [GenerateBind] type`                                                                             
 GP0011  | GenerateBind | Error    | \[FromBody] property must be named 'Body'. `Property '{0}' is annotated with [FromBody] but must be named 'Body' in a [GenerateBind] type`                                                 
