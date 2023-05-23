using JsonSourceGenerator;

var client = new SwaggerClient();
client.PostAddPet(new Pet
{
    Id = 1,
    Category = "Dogs",
    Name = "Fikkie",
    Status = "Begraven"
});