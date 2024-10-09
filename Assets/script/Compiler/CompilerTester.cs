using System;
/*
            effect {
                Name: "Damage",
                Params: {
                    Amount: Number
                },
                Action: (targets, context) => {
                    for target in targets { 
                        int i = 0;
                        while (i++ < Amount) {
                            target.Power -= 1;
                        }
                    }
                }
            }
            effect {
            Name: "Draw",
            Action: (targets, context) => {
            topCard = context.Deck.Pop();
            context.Hand.Add(topCard);
            context.Hand.Shuffle();
            }
            }

            effect {
            Name: "Return to Deck",
            Action: (targets, context) => {
            for target in targets {
            owner = target.Owner;
            deck = context.DeckOfPlayer(owner);
            deck.Push(target);
            deck.Shuffle();
            context.Board.Remove(target);
            };
            }
            }        
            card {
                Name: "Gerald",
                type: "Oro",
                faction: "Magic Knight",
                power: 8,
                range: ["Melee", "Ranged"],
                OnActivation: [
                    {
                        Effect: {
                            Name: "Damage",
                            Amount: 5
                        },
                        Selector: {
                            Source: "board",
                            Single: false,
                            Predicate: (unit) => unit.faction == "Magic" && unit.name != "Gerald"
                        },
                        PostAction: {
                            Effect: "Return to Deck",
                            Selector: {
                                Source: "parent",
                                Single: false,
                                Predicate: (unit) => unit.Power < 1
                            }
                        }
                    },
                    {
                        Effect: "Draw"
                    }
                ]
            }

            effect {
                Name: "Heal",
                Params: {
                    Amount: Number
                },
                Action: (targets, context) => {
                    for target in targets {
                        int i = 0;
                        while (i++ < Amount) {
                            target.Power += 1;
                        }
                    }
                }
            }

            card {
                Name: "Ciri",
                type: "Oro",
                faction: "Magic Knight",
                power: 9,
                range: ["Ranged"],
                OnActivation: [
                    {
                        Effect: {
                            Name: "Heal",
                            Amount: 3
                        },
                        Selector: {
                            Source: "board",
                            Single: false,
                            Predicate: (unit) => unit.faction == "Magic Knight" && unit.Power < 9
                        }
                    },
                    {
                        Effect: "Draw"
                    }
                ]
            }

            card {
                Name: "Triss Merigold",
                type: "Plata",
                faction: "Magic Knight",
                power: 6,
                range: ["Ranged"],
                OnActivation: [
                    {
                        Effect: {
                            Name: "Damage",
                            Amount: 3
                        },
                        Selector: {
                            Source: "board",
                            Single: false,
                            Predicate: (unit) => unit.faction == "Monster"
                        }
                    },
                    {
                        Effect: {
                            Name: "Heal",
                            Amount: 2
                        },
                        Selector: {
                            Source: "board",
                            Single: false,
                            Predicate: (unit) => unit.faction == "Magic Knight"
                        }
                    }
                ]
            }
            
*/