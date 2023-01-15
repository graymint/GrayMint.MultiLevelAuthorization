$projectDir = $PSScriptRoot;

nswag run "$projectDir/Api/Api.nswag"  `
	/variables:namespace=MultiLevelAuthorization.Test.Api,projectDir=$projectDir/../MultiLevelAuthorization.Server/MultiLevelAuthorization.Server.csproj `
	/runtime:Net60
