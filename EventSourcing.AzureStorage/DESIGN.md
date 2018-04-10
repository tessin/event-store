
# First order of business

We need to be able to establish an ordering of events in a distributed environment.

Regardless of things like `lamport timestamps` and `vector clock` the problem we must solve is that if `A` is written into the event store before `B` then `B` must have a higher `Event ID`.

Table storage is great, but I am genuinely worried that it just won't work because it doesn't have the patterns that we typically need for an event store. Like an atomic update operation.

