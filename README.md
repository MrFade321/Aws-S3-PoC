# MeshStreaming POC

## Project Overview

MeshStreaming POC aims to enable runtime downloading of navigation meshes, facilitating the streaming of only the necessary tiles per path request. This approach significantly reduces the initial download size for projects that traditionally include all their navigation mesh tiles upfront. The project leverages the AWS SDK, particularly S3 buckets, for storing and streaming navigation meshes.

## Features

- Downloads navigation mesh tiles dynamically based on path requests.
- Reduces the initial download size by streaming only the required tiles.
- Utilizes AWS S3 buckets for storage and seamless streaming of navigation meshes.

## Pros

- Significantly smaller initial download size compared to pre-packed navigation meshes.
- Users receive only the tiles they request, optimizing disk space and network bandwidth usage.

## Cons

- Longer initial path request times due to the need to download tiles on demand.
- Incurs server hosting costs for maintaining and serving navigation meshes from AWS S3.

## Usage

### Prerequisites

- AWS account with S3 bucket configured to store navigation meshes.
- Access credentials and permissions set up for AWS SDK.

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/yourusername/mesh-streaming-poc.git
   cd mesh-streaming-poc
****
