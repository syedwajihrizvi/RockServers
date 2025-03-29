USER=$1
DATA=$2
URL="http://localhost:5191/api/discussions"
echo "Sending: Data $DATA"
TOKEN=$(grep -o "${USER}:[^,]*," ./logs/usernameToToken.txt | cut -d ':' -f2- | tr -d ' ' | tr -d ',')
echo "Token: $TOKEN"
RESPONSE=$(curl -X POST $URL -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" --data "$DATA" --verbose)
echo "$RESPONSE"
