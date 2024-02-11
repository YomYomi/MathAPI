# Welcome to Rest Calculator!

This is a **Rest based calculator sample** 
The calculator takes 2 floating point numbers in the request body, and  a third operand to determine which operation to perform (Add, Subtract, Multiply or Divide)  and execute the operation on the given numbers.

this is done as an Exercise to demonstrate some features in Swagger, SwaggerHub,  JWT Authentication, and a some Unit test. also as a bonus we put it all in a Docker Image.
the solution  based on .Net 6.0 and  has 2 projects :
- the main project **ClaculatorAPI** is 
-  a Test project **ClaculatorAPI.Tests**

# /api/token endpoint

**
The Api is  implementing the  JWT Bearer Token Authentication.
so before using the Calculator you need to Authenticate using the endpoint above.
you will need to supply 2 *form-data* parameters 
ClientId -  (use : ***sampleClientId*** ) 
 ClientSecret -  (use :  ***sampleClientSecret***).
you can use Postman  or run this *curl*  command
(replace the placeholder uri (localhost:8080) with your uri)

    curl --location 'http://{*localhost:8080*}/api/token' \
    --form 'clientId="sampleClientId"' \
    --form 'clientSecret="sampleClientSecret"'

#

# /api/calculate endpoint
**

as mention we expect 2 parameters that come as an object in the Request body 
CalculatorRequest{
number1,
number2
}
the third is come as custom header: "operation" and  accept values: (multiply, add, subtract, divide) for the operation to perform.

# Docker
to use docker to  run the api use  this command to build the image:

    docker build -t calculatorApi .

and run this command to run the container:

    docker run -d -p 8080:80 --name calculatorApi calculatorApi 
