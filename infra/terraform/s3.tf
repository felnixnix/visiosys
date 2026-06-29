resource "aws_s3_bucket" "documentos" {
  bucket = "visiosys-documentos-${var.environment}-${data.aws_caller_identity.current.account_id}"

  tags = { Name = "visiosys-documentos-${var.environment}" }
}

data "aws_caller_identity" "current" {}

resource "aws_s3_bucket_versioning" "documentos" {
  bucket = aws_s3_bucket.documentos.id
  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "documentos" {
  bucket = aws_s3_bucket.documentos.id
  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "documentos" {
  bucket                  = aws_s3_bucket.documentos.id
  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_lifecycle_configuration" "documentos" {
  bucket = aws_s3_bucket.documentos.id

  rule {
    id     = "mover-para-ia-apos-90-dias"
    status = "Enabled"
    filter {}
    transition {
      days          = 90
      storage_class = "STANDARD_IA"
    }
  }
}

# Política: permite que a EC2 (via IAM role) leia e escreva documentos
resource "aws_iam_role" "ec2_app" {
  name = "visiosys-ec2-app-${var.environment}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Action    = "sts:AssumeRole"
      Effect    = "Allow"
      Principal = { Service = "ec2.amazonaws.com" }
    }]
  })
}

resource "aws_iam_role_policy" "s3_documentos" {
  name = "s3-documentos"
  role = aws_iam_role.ec2_app.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect = "Allow"
      Action = ["s3:PutObject", "s3:GetObject", "s3:DeleteObject", "s3:ListBucket"]
      Resource = [
        aws_s3_bucket.documentos.arn,
        "${aws_s3_bucket.documentos.arn}/*"
      ]
    }]
  })
}

resource "aws_iam_instance_profile" "ec2_app" {
  name = "visiosys-ec2-app-${var.environment}"
  role = aws_iam_role.ec2_app.name
}
