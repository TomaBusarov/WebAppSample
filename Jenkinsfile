#!groovy

node() {
	stage('Get Source Code') {
		checkout scm
		echo "Retrieved $env.BRANCH_NAME in workspace: $env.WORKSPACE"
	}

	// in-script vars
	def commonReleaseBuildParams = "/p:Configuration=Release;Platform=AnyCPU"
	def solutionPath = "$env.WORKSPACE/WebAppSample.sln"
	def msbuild = tool name:'msbuild_2017', type:'msbuild'

	stage('Initialize workspace') {
		echo 'Restore nuget packages for solution'
		bat "Tools\\nuget.exe restore \"$solutionPath\""
	}

	stage ('Build') {
		echo 'Build solution'
		bat "\"${msbuild}\" \"${solutionPath}\" /t:Build $commonReleaseBuildParams /p:Platform=\"Any CPU\"" // override the solution platform
	}

	stage('Run Unit Tests') {
		bat "\"C:\\Program Files (x86)\\Microsoft Visual Studio\\2017\\Professional\\Common7\\IDE\\MSTest.exe\" /resultsfile:TestResults.Contoso.Mathlib.Tests.trx /testcontainer:.\\Contoso.Mathlib.Tests\\bin\\Release\\Contoso.Mathlib.Tests.dll"

		echo 'Publish test results'
		step([$class: 'MSTestPublisher', testResultsFile:"TestResults.*.trx", keepLongStdio: true])
	}
}
