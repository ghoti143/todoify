.PHONY: setup dev lint test test-unit test-int stop

# One command to install all dependencies
setup:
	dotnet restore ./server/src/Todoify.Api
	npm install --prefix ./client

# Start everything
dev:
	dotnet run --project ./server/src/Todoify.Api & \
	npm run dev --prefix ./client & \
	sleep 5 && \
	open http://localhost:5173 && \
	open http://localhost:5231/swagger/index.html

lint:
	dotnet format ./server/src/Todoify.Api --verify-no-changes
	npm run lint --prefix ./client

test: test-unit test-int

test-unit:
	dotnet test ./server/tests/Todoify.Api.UnitTests
	npm run test --prefix ./client

test-int:
	dotnet test ./server/tests/Todoify.Api.IntegrationTests

# Kill processes
stop:
	@pkill -f "Todoify.Api" || true
	@pkill -f "vite" || true
