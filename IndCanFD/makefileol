# Variables
CC = gcc
CFLAGS = -Wall -Wextra -pedantic -Isrc
LIBS = -lsqlite3
SRCDIR = src
OBJDIR = obj
BINDIR = bin
SOURCES_CANGATE = $(wildcard $(SRCDIR)/cangate/*.c)
SOURCES_4REL4IN = $(wildcard $(SRCDIR)/4Rel4In/*.c)
OBJECTS_CANGATE = $(SOURCES_CANGATE:$(SRCDIR)/%.c=$(OBJDIR)/%.o)
OBJECTS_4REL4IN = $(SOURCES_4REL4IN:$(SRCDIR)/%.c=$(OBJDIR)/%.o)

# Targets
$(BINDIR)/cangate: $(OBJECTS_CANGATE)
	mkdir -p $(@D)
	$(CC) $(CFLAGS) -o $@ $^ $(LIBS)

$(BINDIR)/4Rel4In: $(OBJECTS_4REL4IN)
	mkdir -p $(@D)
	$(CC) $(CFLAGS) -o $@ $^ $(LIBS)

# Pattern rules
$(OBJDIR)/%.o: $(SRCDIR)/%.c
	mkdir -p $(@D)
	$(CC) $(CFLAGS) -c $< -o $@

# Cleaning up
clean:
	rm -rf $(OBJDIR) $(BINDIR)
