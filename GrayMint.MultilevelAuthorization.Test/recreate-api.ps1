$projectDir = $PSScriptRoot;
nswag swagger2csclient `
	/runtime:Net60 `
	/input:https://localhost:17443/swagger/v1/swagger.json `
	/output:$projectDir/Apis/Api.cs `
	/namespace:MultiLevelAuthorization.Test.Apis `
	/operationGenerationMode:MultipleClientsFromFirstTagAndPathSegments `
	/jsonLibrary:SystemTextJson `
	/injectHttpClient:true `
    /disposeHttpClient:false `
	/generateOptionalParameters:true `
	/useBaseUrl:false `
	/generateNullableReferenceTypes:false `
	/generateOptionalPropertiesAsNullable:false `
	/dateType:System.DateTime `
	/dateTimeType:System.DateTime `
	/classname:"{controller}Controller" `
	/useBaseUrl:false, `
    /generateBaseUrlProperty:false

