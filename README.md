## basic components monitor github services:
### requirements
* docker

### summary
written in .net6

every 5 seconds the service will poll "[this](https://www.githubstatus.com/api/v2/components.json)" and report of any of the services that is not "operational" both to stdout and service status metrics to http://localhost:8083/metrics

The service should run in k8s.(added pod.yaml)

the service should not "know" slack (separation of concerns), the alerts need to be configured under prometheus/alertmanager or any other monitoring/alerting system.

data is provided through environment variables
```bash
GH_STATUSURL="https://www.githubstatus.com/api/v2/components.json"
GH_MONITORED="API Requests,Codespaces,GitHub Packages"
```



## build
```bash
docker build . -t ghmonitor:0.1
```
run and monitor specific services
```bash
docker run -d -p 8083:8083 -e  GH_STATUSURL="https://www.githubstatus.com/api/v2/components.json" -e GH_MONITORED="API Requests,Codespaces,GitHub Packages" ghmonitor:0.1
```
all services
```bash
docker run -d  -p 8083:8083 -e GH_STATUSURL="https://www.githubstatus.com/api/v2/components.json" -e GH_MONITORED="Git Operations,API Requests,Webhooks,Issues,Pull Requests,GitHub Actions,GitHub Packages,GitHub Pages,Codespaces" ghmonitor:0.1
```

open browser at http://localhost:8083/metrics

CI/CD would include:

* scan(snyk) *.csproj
* test
* build container image
* push to registry
* (deploy) update argo .values file to update service's version to the newly created version


improvements:
stdout report to prometheus /metrics to align with existing tools and systems - DONE