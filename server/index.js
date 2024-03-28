/* eslint-disable no-undef */
const express = require('express')
const path = require('path');

const app = express();

const buildDir = path.join(__dirname, "../build");
console.log('Using files in' + buildDir);

const subDir = '/';
const logRequests = false;

if(subDir === '/'){
    console.log("root");
}
else 
    console.log('not root');

app.use(subDir, express.static(buildDir));

app.get('*', (req, res) => {
    if(logRequests){
        console.log(req.method + ' ' + req.url);
    }
    res.sendFile(path.join(buildDir, 'index.html'));
})

const port = process.env.PORT || 3000;
app.listen(port);

console.log(`app running at port ${port}`);