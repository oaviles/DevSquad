param mylocation string = 'eastus'
param mystorageAccountName string = 'mystoragemx1' // must be globally unique

resource stg 'Microsoft.Storage/storageAccounts@2019-06-01' = {
  name: mystorageAccountName
  location: mylocation
  kind: 'Storage'
  sku: {
    name: 'Standard_LRS'
  }
}
