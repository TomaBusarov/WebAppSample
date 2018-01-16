node {
    stage ('Get Source Code') {
        checkout scm
    }
    stage ('Initialize Workspace') {
        bat "Tools\\nuget.exe restore WebAppSample.sln"
    }
}