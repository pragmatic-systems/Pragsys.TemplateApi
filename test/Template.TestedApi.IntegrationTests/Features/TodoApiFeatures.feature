Feature: TodoApiFeatures

Feature test for the Todo list API.

@todo-list @api
Scenario: Create and View Todo item
	Given We have user 'Dave'
	And User 'Dave' has claims 'ReadWrite'
	When We are connecting as 'Dave'
	And We create task 'New Task'
	Then The response should be 200 OK
	And The response should contain a new RecordId
	When We get our TodoList
	Then The response should be 200 OK
	And The response should contain a Todo List
	Then The result contains the created recordId

@todo-list @api
Scenario: Restricted Write Access
	Given We have user 'James'
	And User 'James' has claims 'Read'
	When We are connecting as 'James'
	And We create task 'New Task'
	Then The response should be 403 Forbidden

@todo-list @api
Scenario: Restricted Anonymous Access
	When We are connecting anonymously
	And We create task 'New Task'
	Then The response should be 401 Unauthorized
