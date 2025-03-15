USER=$1
DATA=$2
PORT=$3
URL="http://localhost:$PORT/api/sessions"
echo "Sending: Data $DATA"
TOKEN=$(grep -o "${USER}:[^,]*," ./logs/usernameToToken.txt | cut -d ':' -f2- | tr -d ' ' | tr -d ',')
echo "Token: $TOKEN"
RESPONSE=$(curl -X POST $URL -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" --data "$DATA")
echo "$RESPONSE"
