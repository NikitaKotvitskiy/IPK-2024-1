PROJECT_NAME = IPK-2024-1

PROJECT_FILE = $(PROJECT_NAME)/$(PROJECT_NAME).csproj

BUILD_FLAGS = --configuration Release

all: build

build:
	dotnet build $(BUILD_FLAGS) $(PROJECT_FILE)

clean:
	dotnet clean $(PROJECT_FILE)