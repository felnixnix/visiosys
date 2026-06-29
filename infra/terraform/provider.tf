terraform {
  required_version = ">= 1.8"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Estado remoto — bucket deve ser criado manualmente antes do primeiro apply.
  # aws s3api create-bucket --bucket visiosys-terraform-state --region sa-east-1 \
  #   --create-bucket-configuration LocationConstraint=sa-east-1
  # aws s3api put-bucket-versioning --bucket visiosys-terraform-state \
  #   --versioning-configuration Status=Enabled
  backend "s3" {
    bucket  = "visiosys-terraform-state"
    key     = "prod/terraform.tfstate"
    region  = "sa-east-1"
    encrypt = true
  }
}

provider "aws" {
  region = var.aws_region
}
