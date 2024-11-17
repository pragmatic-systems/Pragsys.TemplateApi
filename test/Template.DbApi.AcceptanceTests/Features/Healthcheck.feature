Feature: Healthchecks

@healthcheck @api
Scenario: API Healthcheck
	Given We have a Application api
	When We call the ping endpoint
	Then The response should be 200 OK
	When We call the healthcheck endpoint
	Then The response should be 200 OK
	When We call the metrics endpoint
	Then The response should be 200 OK
	When We call the swagger endpoint
	Then The response should be 200 OK
