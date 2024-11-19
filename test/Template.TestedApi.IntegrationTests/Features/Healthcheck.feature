Feature: Healthchecks

@healthcheck @api
Scenario: Ping
	When We call the ping endpoint
	Then The response should be 200 OK
	
Scenario: Healthcheck
	When We call the healthcheck endpoint
	Then The response should be 200 OK
	
Scenario: Metrics
	When We call the metrics endpoint
	Then The response should be 200 OK