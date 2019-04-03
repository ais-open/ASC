# AIS Service Catalog
Publisher: Applied Information Sciences
=========
This repo is for the AIS Azure Service Catalog webapp.

AIS Service Catalog is a product that creates a catalog of technologies pre-approved by IT, allowing application development teams to quickly onboard technologies via a self-service portal. This eliminates the friction – developers can quickly provision technologies and create solutions that meet their deadlines. IT can manage and provision new technologies, enforcing corporate governance. With the AIS Service Catalog, these competing needs can meet in the middle, all while quickly implementing a cloud strategy (ADMIN CREDENTIALS NEEDED).

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://azuredeploy.net/)

## Provision Azure Resources

1. Login to Azure Portal
2. AzureAD App   
    a. Go to Azure Active Directory -> App registrations -> New application registration   
    b. After the app is created, copy the ApplicationID   
    c. Go to Settings -> Keys, and add a new Password, by specifying the Description and Expiration, and click Save   
    d. Make sure to copy the key value.   
    e. The ApplicationID and the Password (key value) will go in the appSettings of the Azure webapp   
3. Storage account   
    a. Provision a RA-GRS storage account   
    b. Note down the Storage account name and one of the Access keys. These will go in the appSettings of the Azure webapp   
4. SendGrid   
    a. Create a new SendGrid account   
    b. After the SendGrid account is created, go to Management portal (<https://app.sendgrid.com/)>   
    c. Go to Settings -> API Keys, and Create an API Key   
    d. Note down the API Key. This will go in the appSettings of the Azure webapp   
5. Azure Webapp   
    a. Create an Azure webapp   
    b. Setup Application Insights and make note of the Instrumentation Key   
6. Azure webapp AppSettings - Add/update these AppSettings for the Azure Webapp

        StorageAccountName - Use the storage account name from #3b
        StorageAccountKey - Use the storage account key from #3b
        ida:ClientID - Use the ApplicationID from #2e
        ida:Password - Use the Password from #2e
        iKey - Use Application Insights Key
        AscAppId - Generate and use a unique guid
        SendGridApiKey - The API key from #4d
        SendGridEndPoint - https:/pi.sendgrid.com
        NotificationFromEmailAddress - Ex:MyServiceCatalog@mydomain.com
        NotificationFromName - Service Catalog 
        AdminEmailAddress - Admin email address

## Deploying Service Catalog

1. It is preferred to setup Azure DevOps pipeline to build and deploy
2. You can also use the Visual Studio to setup the **Publish Profile**  
