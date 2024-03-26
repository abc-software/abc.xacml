## NOT IMPLEMENTED PROFILES

* Multiple Decision Profile #POL01
  -	Xacml30ProtocolSerializer - It is believed that the result can be but one
  -	PolicyInformationPoint - It is believed that the result can be but one, several results are combined into one collection
  -	ReturnPolicyIdList - #SPEC2760
  -	XacmlContextAttributes @id - #SPEC2815
  -	EvaluationEngine30 MakeResponse()

* Administration and Delegation Profile #POL02
  -	PolicySet/Policy - PolicyIssuer
  -	PolicySet/Policy - MaxDelegationDepth

## KNOWN RESTRICTIONS

* [XACMLCore v2, 6.15.] Element &lt;StatusDetail&gt; - do not supported
* [XACMLCore v2, 6.16.] Element &lt;MissingAttributeDetail&gt; - do not supported
* [XACMLCore v2, 6.8.] Element &lt;XacmlContextAttributeValue&gt; support only string value type
* [XACMLCore v2, 6.4.] Element &lt;XacmlContextResourceContent&gt; support only string value type

## TODO
* Methods ReadResource, ReadAction, ReadSubject, ReadEnvonment to one method ReadAllOf

* For XacmlContextAttribute write category value by default.
* Combine classes XacmlContextAction and XACMLContextEnviroment into one
* Combine classes XacmlContextAttributeValue and XacmlContextResourceContent into one
* Combine classes XacmlPolicyCombinerParameters.cs and XacmlPolicySetCombinerParameters.cs into one

* To fix warnings Missing XML comments, StyleCope SA16** rules

### V1.2
- Added supports netstadard1.6 and netstadard2.0

### V1.1
- Removed CodeContracts
- Removed dependency from Abc.Diagnostics library

### V1.0-rc
- Implemented MultipleRequests #SPEC2947
- Fixed status texsts

### V1.0-beta2
- Fixed SonarCube analyser warnings

### V1.0-beta1
- In classes DataTypes/*.cs uses FormatException
- Class EvaluationEngine.cs modificed for MultipleRequests
- Clases XacmlRuleCombinerParameters.cs, XacmlPolicyCombinerParameters.cs, XacmlPolicySetCombinerParameters.cs inherited from XacmlCombinerParameters.cs
- Removed class XacmlCommonAttributeDesignator.cs
- Calss XacmlRule removed property Condition_2_0
- Removed method from interface IXacmlApply.cs
- Class XacmlContextAttributesReference.cs replaces with XacmlContextRequestReference.cs
- Removed class XacmlDefaultType.cs, 
- Classes and interfacces IXPathEngine.cs, IXQueryEngine.cs, NetXPathEngine.cs replaced with XPathProcessor.cs
- Class XacmlTarget contains only XacmlAnyOff
- Class XacmlAllOff contains only XacmlMatch
- Classes XacmlSubject, XacmlResource, XacmlAction, XacmlEnvironment inherited from XacmlAllOff
- Changed namespace for clases Xacml*Exceptions.cs
- Classes XacmlContextAction.cs , XacmlContextEnviroment, XacmlContextSubject.cs, XacmlContextResource.cs inherited from XacmlContextBase.cs
- Removed class XacmlContextDefaults.cs

- Addedd licenses GPL V3.0, LGPL V3.0
- Added headers and StyleCope rules

### V1.0-beta 
- Serializator methods optimization.
	- 1.0 serialzator methods (Write|Read)SubjectMatch, (Write|Read)ResourceMatch, (Write|Read)ActionMatch replaced with (Write|Read)Match
	- 2.0 serialzator methods (Write|Read)EnvironmentMatch replaced with (Write|Read)Match
	- 3.0 serialzator methods (Write|Read)CommonMatch replaced with (Write|Read)Match

	- 1.0 serialzator methods (Write|Read)SubjectAttributeDesignator, (Write|Read)ResourceAttributeDesignator, (Write|Read)ActionAttributeDesignator replaced with (Write|Read)AttributeDesignator
	- 2.0 serialzator methods (Write|Read)EnvironmentAttributeDesignator replaced with (Write|Read)AttributeDesignator
	- 3.0 serialzator methods (Write|Read)CommonAttributeDesignator replaced with (Write|Read)AttributeDesignator

### V1.0-alpha1
- Fixed XacmlAttributeAssignment serialization.

### V1.0-alpha
- Supported Xacml Policy and Xacml Context V1.0/1.1/2.0/V3.0
- Supported Engines V1.1/2.0/3.0