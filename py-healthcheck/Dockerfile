# Use the official Debian image as a parent image
FROM debian:bookworm-slim

# Set the working directory in the container
WORKDIR /app

# Install system dependencies and build tools
RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    python3.11 \
    python3-pip \
    gcc \
    build-essential \
    && apt-get clean && \
    rm -rf /var/lib/apt/lists/*

RUN apt-get update && apt-get install -y python3-dev pkg-config libdbus-1-dev libglib2.0-dev docker.io

# Install Poetry
RUN pip3 install poetry --break-system-packages

# Copy the pyproject.toml and poetry.lock files to the container
COPY pyproject.toml poetry.lock* /app/

# Install the dependencies
RUN poetry install --no-root

# Copy the rest of the application code to the container
COPY . /app

# Set environment variables
ENV PYTHONUNBUFFERED=1

RUN poetry install

# Run the application
CMD ["poetry", "run", "ldn_healthcheck"]
