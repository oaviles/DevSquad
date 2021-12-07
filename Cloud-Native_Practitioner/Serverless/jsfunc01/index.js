module.exports = async function (context, req)  {
    context.log('JavaScript HTTP trigger function processed a request.');

    const event = (req.query.event || (req.body && req.body.event));
    const guid =  createUUID();
    context.log('OA-Event:' + guid);

    let appInsights = require("applicationinsights");
    const appinsightskey= process.env["APPINSIGHTS_INSTRUMENTATIONKEY"];
    appInsights.setup(appinsightskey).start();
    let client = appInsights.defaultClient;
    client.trackEvent({event: "MyEventDemo", properties: {guid: guid, event: event}});

    if (event) {
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: "OA-Event: " + event + " | GUID: " + guid
        };
    }
    else {
        context.res = {
            status: 400,
            body: "Please pass a event on the query string or in the request body"
        };
    }

    function createUUID() {
        return 'oaxxxxxx-xxxx-6xxx-yxyx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
           var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
           return v.toString(16);
        });
     }
}