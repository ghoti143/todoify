.PHONY: dev stop
 
dev:
	dotnet run --project ./server/src/Todoify.Api & npm run dev --prefix ./client
 
stop:
	@pkill -f "dotnet run" || true
	@pkill -f "vite" || true