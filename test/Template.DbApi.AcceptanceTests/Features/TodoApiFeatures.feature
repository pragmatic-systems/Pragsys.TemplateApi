Feature: TodoApiFeatures

Feature test for the Todo list API.

@todo-list @api
Scenario: Create and View Todo item
	Given We have a Application api
	When We create task 'New Task'
	Then The response should be 200 OK
	And The response should contain a new RecordId
	When We get our TodoList
	Then The response should be 200 OK
	And The response should contain a Todo List
	Then The result contains the created recordId
