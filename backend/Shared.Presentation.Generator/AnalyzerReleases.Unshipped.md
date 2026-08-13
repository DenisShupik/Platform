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
 GP0100  | GenerateEndpoint | Error | Invalid generated endpoint specification. `Endpoint '{0}' is invalid: {1}`
 GP0101  | GenerateEndpoint | Error | Application request property cannot be mapped. `Endpoint '{0}' cannot map application property '{1}' from request '{2}'`
 GP0102  | GenerateEndpoint | Error | Endpoint documentation is missing. `Documentation operation '{0}' was not found in Documentation/Api.en.xml`
 GP0103  | GenerateEndpoint | Error | Endpoint documentation is invalid. `Cannot read endpoint documentation: {0}`
 GP0104  | GenerateEndpoint | Error | Endpoint documentation key is duplicated. `Documentation operation key '{0}' is declared more than once`
 GP0105  | GenerateEndpoint | Warning | Endpoint documentation is unused. `Documentation operation '{0}' does not have a generated endpoint`
 GP0106  | GenerateEndpoint | Error | Route parameter is not bound by the request. `Endpoint '{0}' route parameter '{1}' does not have a matching [FromRoute] property on request '{2}'`
 GP0107  | GenerateEndpoint | Error | Request route property is missing from the route. `Endpoint '{0}' [FromRoute] property '{1}' does not have a matching parameter in the complete route pattern`
 GP0108  | GenerateEndpoint | Error | CreatedAt target cannot be inferred. `Endpoint '{0}' cannot infer a unique GET endpoint for created value '{1}': {2}`
 GP0109  | GenerateEndpoint | Error | CreatedAt target is invalid. `Endpoint '{0}' CreatedAt request '{1}' must be a registered GET endpoint accepting the created value '{2}' as its only [FromRoute] property`
 GP0110  | GenerateEndpoint | Error | Endpoint name must be globally unique. `Generated endpoint name '{0}' is used by more than one endpoint`
 GP0111  | GenerateEndpoint | Error | Handler kind does not match the HTTP method. `Endpoint '{0}' mapped with {1} must use a {2} handler, but '{3}' is a {4} handler`
 GP0112  | GenerateEndpoint | Warning | Endpoint types do not follow the same naming convention. `HTTP request '{0}', application request '{1}', and handler '{2}' should use the same operation stem`
 GP0113  | GenerateEndpoint | Warning | Route-group child pattern should not start with a slash. `Endpoint '{0}' is mapped on a route group, so its pattern should not start with '/'`
 GP0114  | GenerateEndpoint | Error | Optional route parameter cannot bind to a generated request. `Endpoint '{0}' route parameter '{1}' is optional, but [FromRoute] property '{2}' is required by generated binding`
