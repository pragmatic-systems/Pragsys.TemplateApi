Feature: CsvUploadImport

Feature test for uploading a CSV file and importing the results via a background job.

@csv @api @storage
Scenario: Upload CSV and import todo items via background job
	Given We have user 'Alice'
	And User 'Alice' has claims 'ReadWrite'
	When We are connecting as 'Alice'
	And We upload a CSV file with three todo items
	Then The response should contain a blob name
	When We wait for the background job to complete
	And We get our TodoList
	Then The response should contain a Todo List
	And The response should contain at least 3 todo items
