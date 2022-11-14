#Start sql service first
docker-compose up -d sql

#Wait 2 minutes for database created
sleep 2m

#Start web service
docker-compose up -d web