terraform {
   required_providers {
    azurerm = "~> 2.11.0"
  }
  backend "remote" {
    # hostname     = "app.terraform.io"
    organization = "your-organization-on-terraform-cloud"
    workspaces {
      name = "your-workspace-on-terraform-cloud"
    }
  }
}

provider "azurerm" {
  # The "feature" block is required for AzureRM provider 2.x.
  # If you're using version 1.x, the "features" block is not allowed.
  skip_provider_registration = "true"
  features {}
  subscription_id = var.subscription_id
}

resource "azurerm_resource_group" "rg" {
  name     = "IaC_Terraform"
  location = "East US"
}

resource "azurerm_storage_account" "storage1" {
  name                     = "storageaccountmx4"
  resource_group_name      = azurerm_resource_group.rg.name
  location                 = azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    environment = "demo"
  }
}
