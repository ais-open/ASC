# AIS Service Catalog
Publisher: Applied Information Sciences
=========
This repo is for the AIS Azure Service Catalog webapp.

AIS Service Catalog is a product that creates a catalog of technologies pre-approved by IT, allowing application development teams to quickly onboard technologies via a self-service portal. This eliminates the friction – developers can quickly provision technologies and create solutions that meet their deadlines. IT can manage and provision new technologies, enforcing corporate governance. With the AIS Service Catalog, these competing needs can meet in the middle, all while quickly implementing a cloud strategy (ADMIN CREDENTIALS NEEDED).

<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAppliedIS%2FASC%2Fasc-jagrati%2FDeploy%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

## Provision Azure Resources

1. Login to Azure Portal
2. AzureAD App   
    a. Go to Azure Active Directory -> App registrations -> New application registration   
    b. After the app is created, copy the ApplicationID   
    c. Go to Settings -> Keys, and add a new Password, by specifying the Description and Expiration, and click Save   
    d. Make sure to copy the key value.   
    e. The ApplicationID and the Password (key value) will go in the appSettings of the Azure webapp   
3. SendGrid   
    a. Create a new SendGrid account   
    b. After the SendGrid account is created, go to Management portal (<https://app.sendgrid.com/)>   
    c. Go to Settings -> API Keys, and Create an API Key   
    d. Note down the API Key. This will go in the appSettings of the Azure webapp 
