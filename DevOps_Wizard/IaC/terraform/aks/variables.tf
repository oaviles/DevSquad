variable "subscription_id" {
  description = "Azure Subscription Id"
  type        = string
}

variable "resource_group" {
  description = "IaC_Workshop Resource Group"
  type        = string
  default     = "IaC_Terraform_RG"
}
