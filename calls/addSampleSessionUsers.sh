SESSION_OWNER_USERNAME=$1
PORT=$2
SESSION_ID=$3
USERS_TO_ADD=$(python3 ../mockData/addSessionUsers.py -u $SESSION_OWNER_USERNAME)
echo "Users to add: $USERS_TO_ADD"
IFS=',' read -a array <<< "$USERS_TO_ADD"
for USER_TO_ADD in "${array[@]}"
do
	echo "Calling for $USER_TO_ADD"
	RESPONSE=$(./addToSession.sh $USER_TO_ADD $PORT $SESSION_ID)
	echo "RESPONSE: $RESPONSE"
done
