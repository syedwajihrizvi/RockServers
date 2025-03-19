USERNAME=$1
PORT=$2
SESSION_ID=$3
TOKEN=$(grep -o "$USERNAME:[^,]*," ./logs/usernameToToken.txt | cut -d ':' -f2- | tr -d ',')
URL="http://localhost:$PORT/api/sessions/$SESSION_ID/addUser"
RESPONSE=$(curl -X PATCH $URL -H "Authorization: Bearer $TOKEN")
echo "$RESPONSE"
