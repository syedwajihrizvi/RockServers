while IFS= read -r line; do
	user=$(echo "$line" | cut -d',' -f1)
	data=$(echo "$line" | cut -d',' -f2-)
	echo "User: $user"
	echo "Data: $data"
	if [[ "${#user}" -gt 0 && "${#data}" -gt 0 ]]; then
		./createDiscussion.sh "$user" "$data"
	fi
done < ../mockdata/mockdiscussions.txt
