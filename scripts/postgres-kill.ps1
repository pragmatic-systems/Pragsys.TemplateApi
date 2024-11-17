
$container = $(docker container ls -a -q --filter=name='Template.DbApi.Db')

# Remove DB containers
if ($container -ne $null) {
    Write-Host "Found existing database container. Removing."
    docker kill $container
    docker rm $container
}
else
{
    Write-Host "No database container found."
}

# Clear out DB image
$image = docker images -a -q --filter=reference='*enricher.db'

if ($image -ne $null) {
    Write-Host "Found existing database image. Removing." 
    docker rmi $image -f
}
else
{
    Write-Host "No database image found."
}