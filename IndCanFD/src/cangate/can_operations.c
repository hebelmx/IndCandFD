#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include <linux/can.h>
#include <stdlib.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <sys/ioctl.h>
#include <net/if.h>
#include <linux/can.h>
#include <linux/can/raw.h>
#include "can_operations.h"


void print_can_frame(struct can_frame *cf) {
    printf("CAN ID: %u\n", cf->can_id);
    printf("CAN data length: %u\n", cf->len);
    for(int i = 0; i < cf->len; i++) {
        printf("Data[%d]: %u\n", i, cf->data[i]);
    }
}

int send_frame(struct can_frame *frame_to_send, char* interface_name){
    int s; // Socket descriptor
    struct sockaddr_can addr;
    struct ifreq ifr;

    // Create a socket
    if ((s = socket(PF_CAN, SOCK_RAW, CAN_RAW)) < 0) {
        perror("Error creating socket");
        return 1;
    }

     // Specify the interface name
    strcpy(ifr.ifr_name, interface_name);

    // Specify the interface name
    //strcpy(ifr.ifr_name, "vcan0");


    // Retrieve the interface index
    if (ioctl(s, SIOCGIFINDEX, &ifr) < 0) {
        perror("Error retrieving interface index");
        close(s);
        return 1;
    }

    // Bind the socket to the CAN interface
    addr.can_family = AF_CAN;
    addr.can_ifindex = ifr.ifr_ifindex;
    if (bind(s, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("Error binding socket");
        close(s);
        return 1;
    }

    // Send the CAN frame
    if (write(s, frame_to_send, sizeof(struct can_frame)) != sizeof(struct can_frame)) {
        perror("Error sending CAN frame");
        close(s);
        return 1;
    }

    // Close the socket
    close(s);

    printf("CAN frame sent successfully.\n");

    return 0;
}


int send_harcoded_frame(){
      int s; // Socket descriptor
    struct sockaddr_can addr;
    struct ifreq ifr;

    // Create a socket
    if ((s = socket(PF_CAN, SOCK_RAW, CAN_RAW)) < 0) {
        perror("Error creating socket");
        return 1;
    }

    // Specify the interface name
    strcpy(ifr.ifr_name, "vcan0");

    // Retrieve the interface index
    if (ioctl(s, SIOCGIFINDEX, &ifr) < 0) {
        perror("Error retrieving interface index");
        close(s);
        return 1;
    }

    // Bind the socket to the CAN interface
    addr.can_family = AF_CAN;
    addr.can_ifindex = ifr.ifr_ifindex;
    if (bind(s, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("Error binding socket");
        close(s);
        return 1;
    }

    // Prepare a CAN frame to send
    struct can_frame frame;
    frame.can_id = 0x001;  // CAN identifier
    frame.can_dlc = 8;     // Data length code (number of bytes)
    frame.data[0] = 0x01;  // Data byte 1
    frame.data[1] = 0x02;  // Data byte 2
    // ...
    // Set other data bytes

    // Send the CAN frame
    if (write(s, &frame, sizeof(frame)) != sizeof(frame)) {
        perror("Error sending CAN frame");
        close(s);
        return 1;
    }

    // Close the socket
    close(s);

    printf("CAN frame sent successfully.\n");

    return 0;
}


int send_can_frame(const void *frame, size_t size) {

  int s; // Socket descriptor
    struct sockaddr_can addr;
    struct ifreq ifr;

    // Create a socket
    if ((s = socket(PF_CAN, SOCK_RAW, CAN_RAW)) < 0) {
        perror("Error creating socket");
        return 1;
    }

    // Specify the interface name
    strcpy(ifr.ifr_name, "vcan0");

    // Retrieve the interface index
    if (ioctl(s, SIOCGIFINDEX, &ifr) < 0) {
        perror("Error retrieving interface index");
        close(s);
        return 1;
    }

    // Bind the socket to the CAN interface
    addr.can_family = AF_CAN;
    addr.can_ifindex = ifr.ifr_ifindex;
    if (bind(s, (struct sockaddr *)&addr, sizeof(addr)) < 0) {
        perror("Error binding socket");
        close(s);
        return 1;
    }

   

 // Send the CAN frame
    if (write(s, frame, size) != size) {
        perror("Error sending CAN frame, Error");
        close(s);
        return 1;
    }

    // Close the socket
    close(s);

    printf("CAN frame sent successfully.\n");

    return 0;

}

int read_and_compare_can_frame(int s, struct can_frame* frame) {
    struct can_frame recv_frame;
    if (read(s, &recv_frame, sizeof(struct can_frame)) != sizeof(struct can_frame)) {
        perror("Error while reading message");
        return -1;
    }
    if (recv_frame.can_id != frame->can_id || recv_frame.can_dlc != frame->can_dlc ||
        memcmp(recv_frame.data, frame->data, frame->can_dlc) != 0) {
        fprintf(stderr, "Error: sent and received frames don't match\n");
        return -1;
    }
    return 0;
}
