#!groovy
// Jenkins pipeline script for Continuous Delivery
// Author: Toma Bussarov, Fourth Bulgaria

// Prerequisites for running this pipeline script:
// - Jenkins slave with Windows and Visual Studio 2017 Pro
// - GitHub branch source plugin
// - MSBuild plugin; configure tool with name 'msbuild_2017' (Manage Jenkins -> Global Tool Configuration)
// - SonarQube Scanner plugin; configure server with name 'SonarQube Server' (Manage Jenkins -> Configure System)
// - Configure SonarQube Scanner for MSBuild tool with name 'SonarScannerMsBuild' (Manage Jenkins -> Global Tool Configuration)
// - HTTP Request Plugin
// - Credentials:
//		SonarQube-github-apikey - authentication token for SonarQube and its GitHub plugin (required permissions: repo, read:public_key)
//		Slack-webhookIn-url - webhook URL to post on Slack
//		Octopus-apikey - authentication token to push packages and create releases in Octopus
// Values to change in script:
//	Path to MsTest.exe
// 	Octopus server URL
//	Solution and project names

// Helper to distinguish between run for Pull Requests or release (package and publish)
def onReleaseBranch() {
	return env.BRANCH_NAME == 'master'
}

node() {
	try { // catch errors in pipeline, so they can be reported to external tools

	stage('Get Source Code') {
		checkout scm
		echo "Retrieved $env.BRANCH_NAME in workspace: $env.WORKSPACE"
	}

	// in-script vars
	def buildVersion = "1.3.${env.BUILD_ID}"
	def commonReleaseBuildParams = "/p:Configuration=Release;Platform=AnyCPU"
	def solutionPath = "$env.WORKSPACE/WebAppSample.sln"
	def msbuild = tool name:'msbuild_2017', type:'msbuild'
	def sqScannerMsBuildHome = tool 'SonarScannerMsBuild'

	stage('Initialize workspace') {
		echo 'Restore nuget packages for solution'
		bat "Tools\\nuget.exe restore \"$solutionPath\""

		echo 'Enable SonarQube analysis'
		withSonarQubeEnv('SonarQube Server') {
			withCredentials([string(credentialsId: 'SonarQube-github-apikey', variable: 'sonarApiKey')]) {
				def sonarPRoptions = ''
				if (!onReleaseBranch()) {
					// add options for Pull Requests
					sonarPRoptions = "/d:sonar.analysis.mode=preview /d:sonar.github.pullRequest=$env.CHANGE_ID /d:sonar.github.repository=TomaBusarov/WebAppSample /d:sonar.github.oauth=$sonarApiKey"
				}
				bat "${sqScannerMsBuildHome}/SonarQube.Scanner.MSBuild.exe begin /k:Contoso.WebAppSample /n:\"WebAppSample\" /v:$buildVersion" + 
					" $sonarPRoptions /d:sonar.host.url=%SONAR_HOST_URL% /d:sonar.cs.vstest.reportsPaths=\"TestResults.*.trx\""
			}
		}
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

	currentBuild.description = 'Binaries built and tested'

	stage('SonarQube Analysis') {
		withSonarQubeEnv('SonarQube Server') {
			bat "${sqScannerMsBuildHome}\\SonarQube.Scanner.MSBuild.exe end"
		}
	}

	if (onReleaseBranch()) {
		// Package and create a release in Octopus when running for release branch

		stage('Package and Publish') {
			echo 'Generate Web application output for packaging'

			def webAppPackageBaseName = 'WebAppSample.ContosoUniversity'
			bat "\"${msbuild}\" ContosoUniversity/ContosoUniversity.csproj /t:Build" + 
				" ${commonReleaseBuildParams} /p:DeployOnBuild=true;PublishProfile=PublishToFolder;DeployTarget=WebPublish"

			echo 'Package Web Application'
			bat "Tools\\Octo.exe pack --id=$webAppPackageBaseName --version=$buildVersion --outFolder=. --basePath=\"$env.WORKSPACE/ContosoUniversity/publish\""

			currentBuild.displayName = '# ' + buildVersion
			currentBuild.description = "Packages for version $buildVersion created" 

			def releaseNotesMdFile = "$env.WORKSPACE/Auto-ReleaseNotes.md"
			generateMarkdownReleaseNotes(buildVersion, releaseNotesMdFile)

			withCredentials([string(credentialsId: 'Octopus-apikey', variable: 'octoApiKey')]) {
				def octopusServerURL = 'http://jp-demo.westeurope.cloudapp.azure.com'
				
				echo 'Publish packages to Octopus'
				bat "Tools\\Octo.exe push" +
					" --package=${webAppPackageBaseName}.${buildVersion}.nupkg" + 
					" --server=${octopusServerURL} --apiKey=${octoApiKey}"
				
				echo 'Create project release'
				bat "Tools\\Octo.exe create-release --project \"Contoso University Web\" --version ${buildVersion}" + 
					" --package=${webAppPackageBaseName}:${buildVersion}" + 
					" --releasenotesfile=\"${releaseNotesMdFile}\"" +
					" --server=${octopusServerURL} --apiKey=${octoApiKey}"
			}
			currentBuild.description = "Release $buildVersion created in Octopus"
		}

		stage('Send Notifications') {
			sendTeamNotification("Release $buildVersion created successfuly.")
		}
	}
	}catch (ex) {
		def errorText = "$ex"
		if (onReleaseBranch()) {
			// notify in case of error
			echo "Error in build job on release branch. Details: $errorText"
			sendTeamNotification("Error in release build job! Details: ${errorText}.")
		}
		throw ex
	}
}


@NonCPS

def getPassedBuilds() {
	def passedBuilds = []

	// Add current build
	passedBuilds.add(currentBuild)
	def	build = currentBuild.previousBuild
	// Add all previous unsuccessful builds
	while ((build != null) && (build.result != 'SUCCESS'))  {
		passedBuilds.add(build)
		build = build.previousBuild
	}
	return passedBuilds
}

def getBuildChangeString(build, itemBegin, itemEnd) {
	MAX_MSG_LEN = 192

	def changeString = ""

	def changeLogSets = build.changeSets
	for (int i = 0; i < changeLogSets.size(); i++) {
		def entries = changeLogSets[i].items
		for (int j = 0; j < entries.length; j++) {
			def entry = entries[j]
			truncated_msg = entry.msg.take(MAX_MSG_LEN).replaceAll('\r', ' ').replaceAll('\n', ' ')
			changeString += "${itemBegin}${entry.author}: ${entry.msg}${itemEnd}"
		}
	}
	return changeString
}

def getChangeString(itemBegin, itemEnd) {
	def accumulatedChanges = ""
	
	def passedBuilds = getPassedBuilds()
	for (int x = 0; x < passedBuilds.size(); x++) {
		accumulatedChanges += getBuildChangeString(passedBuilds[x], itemBegin, itemEnd)
	}

	if (!accumulatedChanges) {
		accumulatedChanges = "${itemBegin}No changes${itemEnd}"
	}
	return accumulatedChanges
}

def generateMarkdownReleaseNotes (buildVersion, fileAndPath) 
{
	def changes = getChangeString('* ', '\n')
	def fileContents = """
##Contoso University WebApp v.$buildVersion
Changes included
$changes
	"""
	writeFile encoding: 'utf-8', file: fileAndPath, text: fileContents
}

def sendTeamNotification(text)
{
	def slackMsgPayload = """{
		"username" : "Demo Jenkins",
		"text" : "$text \\n <${env.JOB_DISPLAY_URL}|View Build>"
	}"""

	withCredentials([string(credentialsId: 'Slack-webhookIn-url', variable: 'slackWebhook')]) {
		httpRequest contentType: 'APPLICATION_JSON', 
					httpMode: 'POST', 
					requestBody: slackMsgPayload, 
					responseHandle: 'NONE', 
					url: slackWebhook
	}
}
