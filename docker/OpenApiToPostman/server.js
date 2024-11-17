'use strict';

// https://github.com/postmanlabs/OpenAPI-to-Postman
const fs = require('fs');
const converter = require('openapi-to-postmanv2');

var files = fs.readdirSync('/swagger');

files.forEach(file => {
    console.log(file);

    var postmanFile = file.replace('swagger', 'collection');
    const openapiData = fs.readFileSync(`/swagger/${file}`, { encoding: 'UTF8' });

    converter.convert({ type: 'string', data: openapiData },
        {}, (err, conversionResult) => {

            if (!conversionResult.result) {
                throw err;
            }

            var payload = conversionResult.output[0].data;
            console.log('The collection object is: ', payload);

            fs.writeFileSync(`/postman/${postmanFile}`, JSON.stringify(payload));
        }
    );

});

