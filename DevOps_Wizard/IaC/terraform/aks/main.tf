terraform {
   required_providers {
    azurerm = "~> 2.11.0"
  }
  backend "remote" {
    # hostname     = "app.terraform.io"
    organization = "personal-mobile"
    workspaces {
      name = "gh-actions-demo"
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

resource "azurerm_kubernetes_cluster" "aks" {
  name                = "AKSCluster-Demo"
  location            = azurerm_resource_group.rg.location
  resource_group_name = var.resource_group
  dns_prefix          = "aksclusterdemo"

  default_node_pool {
    name       = "default"
    node_count = 1
    vm_size    = "Standard_D2_v2"
  }

  identity {
    type = "SystemAssigned"
  }

  tags = {
    Environment = "Demo"
  }
}

output "client_certificate" {
  value = azurerm_kubernetes_cluster.aks.kube_config.0.client_certificate
}

output "kube_config" {
  value = azurerm_kubernetes_cluster.aks.kube_config_raw

  sensitive = true
}
