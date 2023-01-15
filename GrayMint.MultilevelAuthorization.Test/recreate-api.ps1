$projectDir = $PSScriptRoot;

nswag run "$projectDir/Api/Api.nswag"  `
	/variables:namespace=MultiLevelAuthorization.Test.Api,projectDir=$projectDir/../GrayMint.MultiLevelAuthorization.Server/GrayMint.MultiLevelAuthorization.Server.csproj `
	/runtime:Net60
