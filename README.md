# WebAppSample
The repo contains sample web application, used to demonstrate build and deployment pipeline for .Net application.

The most interesting part is the Jenkinsfile in the root, representing applicable to real life build process.
## Jenkinsfile features
- Comment block describes all the Jenkins plugins you need to run the pipeline
- Run for pull requests or create new release
- Building a .Net application
- Run Unit Tests for MSTest Framework and import results in Jenkins
- Publish and package ASP.Net web application (with precompilation!)
- Push to Octopus Deploy and create a release
- Build a "release notes" list from the Git commit messages and attach to Octopus release
- Integration with SonarQube for source code analysis (for release and GitHub pull requests)
- Integration with Slack to report pipeline result
