import socket
import json
import numpy as np
import cv2

# Define server address and port
HOST = '0.0.0.0'  # Set to localhost to accept all incoming connections
PORT = 65432  # Port to listen on (non-privileged ports are > 1023)



def compute_transformation(data):
    real_world_coords = np.array(data['real_world_coords'], dtype=np.float32)
    image_coords = np.array(data['image_coords'], dtype=np.float32)
    keypoints = np.array(data['keypoints'], dtype=np.float32)

    # Compute homography matrix
    H, status = cv2.findHomography(image_coords, real_world_coords, cv2.RANSAC, 4.0)

    # Apply homography to keypoints
    keypoints_transformed = cv2.perspectiveTransform(keypoints.reshape(-1, 1, 2), H)

    transformed_keypoints = [
        {"x": float(point[0][0]), "y": float(point[0][1])} for point in keypoints_transformed
    ]

    return {
        "transformed_keypoints": transformed_keypoints
    }


# Set up server socket
with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as server_socket:
    server_socket.bind((HOST, PORT))
    server_socket.listen()
    print(f"Server listening on {HOST}:{PORT}")

    while True:
        conn, addr = server_socket.accept()
        with conn:
            print(f"Connected by {addr}")
            data = conn.recv(4096)
            if not data:
                break

            # Parse incoming data
            request = json.loads(data.decode('utf-8'))
            response = compute_transformation(request)

            # Send response
            conn.sendall(json.dumps(response).encode('utf-8'))

