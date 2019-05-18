// opt in to Hot Module Replacement
const c: any = (<any>module)["hot"];
if (c) {
    c.accept();
}

// check if app is reloaded or not
const rootNode = document.querySelector("div#root");
const reloaded = rootNode.hasChildNodes();

import 'bulma/bulma.sass';
import '@fortawesome/fontawesome-free/css/all.css';

import { Subject, combineLatest, of, merge, Observable, forkJoin } from 'rxjs';
import { map, distinctUntilChanged, startWith, scan, pairwise, first, debounceTime, filter, catchError, mergeMap, take } from 'rxjs/operators';
import { ajax } from 'rxjs/ajax';
import { h, diff, patch } from 'virtual-dom';
import { HubConnectionBuilder } from '@aspnet/signalr';

// backend services
const shopService = {
    url: 'http://localhost:50324/',
};
const inventoryService = {
    url: 'http://localhost:50330/',
};
const deliveryService = {
    url: 'http://localhost:50336/',
};
const eventHistoryService = {
    url: 'http://localhost:50815/',
};

// models
type Item = {
    id: string;
    name: string;
    price: number;
    remainingQuantity: number;
};

type Cart = {
    items: {
        quantity: number,
        itemId: string
    }[];
};

type Order = {
    id: string;
    number: number;
    createdDate: Date;
    isConfirmed: boolean;
    isCanceled: boolean;
    items: {
        quantity: number,
        price: number,
        itemId: string
    }[];
};

type Event = {
    id: string;
    number: number;
    eventName: string;
    data: any;
    metadata: {
        createdDate: Date,
        correlationId?: string | undefined
    };
};

// actions/events
type ChangePageAction = {
    type: "APP_CHANGE_PAGE",
    pageName: "shop" | "orders" | "inventory" | "events"
};
type UpdateSearchAction = {
    type: "APP_UPDATE_SEARCH",
    search: string
};
type UpdateFormAction = {
    type: "APP_UPDATE_FORM_ACTION",
    formName: "createItem",
    property: string,
    value: any,
    extendedProps: any
};
type ClearFormAction = {
    type: "APP_CLEAR_FORM_ACTION",
    formName: "createItem"
};

type AppLoadStartedAction = {
    type: "APP_LOAD_STARTED"
};
type AppLoadSucceedAction = {
    type: "APP_LOAD_SUCCEED",
    items: Item[],
    cart: Cart,
    orders: Order[],
    events: Event[]
};
type AppLoadFailedAction = {
    type: "APP_LOAD_FAILED",
    error: any
};

type AddItemInCartStartedAction = {
    type: "SHOP_ADD_ITEM_IN_CART_STARTED",
    payload: { itemId: string, quantity: number }
};
type AddItemInCartSucceedAction = {
    type: "SHOP_ADD_ITEM_IN_CART_SUCCEED",
    payload: { itemId: string, quantity: number }
};
type AddItemInCartFailedAction = {
    type: "SHOP_ADD_ITEM_IN_CART_FAILED",
    error: any
};

type RemoveItemFromCartStartedAction = {
    type: "SHOP_REMOVE_ITEM_FROM_CART_STARTED",
    payload: { itemId: string, quantity: number }
};
type RemoveItemFromCartSucceedAction = {
    type: "SHOP_REMOVE_ITEM_FROM_CART_SUCCEED",
    payload: { itemId: string, quantity: number }
};
type RemoveItemFromCartFailedAction = {
    type: "SHOP_REMOVE_ITEM_FROM_CART_FAILED",
    error: any
};

type OrderStartedAction = {
    type: "SHOP_ORDER_STARTED"
};
type OrderSucceedAction = {
    type: "SHOP_ORDER_SUCCEED"
};
type OrderFailedAction = {
    type: "SHOP_ORDER_FAILED",
    error: any
};

type ResetCartStartedAction = {
    type: "SHOP_RESET_CART_STARTED"
};
type ResetCartSucceedAction = {
    type: "SHOP_RESET_CART_SUCCEED"
};
type ResetCartFailedAction = {
    type: "SHOP_RESET_CART_FAILED",
    error: any
};

type UpsertCartAction = {
    type: "SHOP_UPSERT",
    itemAndQuantity: { itemId: string, quantity: number }
};

type ValidateOrderStartedAction = {
    type: "ORDER_VALIDATE_STARTED",
    payload: { orderId: string }
};
type ValidateOrderSucceedAction = {
    type: "ORDER_VALIDATE_SUCCEED"
};
type ValidateOrderFailedAction = {
    type: "ORDER_VALIDATE_FAILED",
    error: any
};

type CancelOrderStartedAction = {
    type: "ORDER_CANCEL_STARTED",
    payload: { orderId: string }
};
type CancelOrderSucceedAction = {
    type: "ORDER_CANCEL_SUCCEED"
};
type CancelOrderFailedAction = {
    type: "ORDER_CANCEL_FAILED",
    error: any
};

type UpsertOrderAction = {
    type: "ORDER_UPSERT",
    order: Order
};

type CreateItemStartedAction = {
    type: "INVENTORY_CREATE_ITEM_STARTED",
    payload: any
};
type CreateItemSucceedAction = {
    type: "INVENTORY_CREATE_ITEM_SUCCEED"
};
type CreateItemFailedAction = {
    type: "INVENTORY_CREATE_ITEM_FAILED",
    error: any
};

type UpdatePriceStartedAction = {
    type: "INVENTORY_UPDATE_PRICE_STARTED",
    payload: { itemId: string, newPrice: number }
};
type UpdatePriceSucceedAction = {
    type: "INVENTORY_UPDATE_PRICE_SUCCEED"
};
type UpdatePriceFailedAction = {
    type: "INVENTORY_UPDATE_PRICE_FAILED",
    error: any
};

type SupplyItemStartedAction = {
    type: "INVENTORY_SUPPLY_ITEM_STARTED",
    payload: { itemId: string, quantity: number }
};
type SupplyItemSucceedAction = {
    type: "INVENTORY_SUPPLY_ITEM_SUCCEED",
    itemId: string
};
type SupplyItemFailedAction = {
    type: "INVENTORY_SUPPLY_ITEM_FAILED",
    error: any
};

type UpsertItemAction = {
    type: "INVENTORY_UPSERT_ITEM",
    item: Item
};

type AddEventAction = {
    type: "EVENT_ADD",
    event: Event
};

type AppAction =
    ChangePageAction | UpdateSearchAction
    | UpdateFormAction | ClearFormAction
    | AppLoadStartedAction | AppLoadSucceedAction | AppLoadFailedAction;

type ShopAction =
    AddItemInCartStartedAction | AddItemInCartSucceedAction | AddItemInCartFailedAction
    | RemoveItemFromCartStartedAction | RemoveItemFromCartSucceedAction | RemoveItemFromCartFailedAction
    | OrderStartedAction | OrderSucceedAction | OrderFailedAction
    | ResetCartStartedAction | ResetCartSucceedAction | ResetCartFailedAction
    | UpsertCartAction;

type DeliveryAction =
    ValidateOrderStartedAction | ValidateOrderSucceedAction | ValidateOrderFailedAction
    | CancelOrderStartedAction | CancelOrderSucceedAction | CancelOrderFailedAction
    | UpsertOrderAction;

type InventoryAction =
    CreateItemStartedAction | CreateItemSucceedAction | CreateItemFailedAction
    | UpdatePriceStartedAction | UpdatePriceSucceedAction | UpdatePriceFailedAction
    | SupplyItemStartedAction | SupplyItemSucceedAction | SupplyItemFailedAction
    | UpsertItemAction;

type EventAction =
    AddEventAction;

type Action =
    AppAction
    | ShopAction
    | DeliveryAction
    | InventoryAction
    | EventAction;

const actionsCreator = {
    app: {
        changePage: (pageName: string) => <ChangePageAction>({ type: "APP_CHANGE_PAGE", pageName }),
        updateSearch: (search: string) => <UpdateSearchAction>({ type: "APP_UPDATE_SEARCH", search }),
        updateForm: (formName: string, property: string, value: any, extendedProps: any = {}) =>
            <UpdateFormAction>({ type: "APP_UPDATE_FORM_ACTION", formName, property, value, extendedProps }),
        clearForm: (formName: string) => <ClearFormAction>({ type: "APP_CLEAR_FORM_ACTION", formName }),
        load: {
            started: () => <AppLoadStartedAction>({ type: "APP_LOAD_STARTED" }),
            succeed: (items: Item[], cart: Cart, orders: Order[], events: Event[]) =>
                <AppLoadSucceedAction>({ type: "APP_LOAD_SUCCEED", items, cart, orders, events }),
            failed: (error: any) => <AppLoadFailedAction>({ type: "APP_LOAD_FAILED", error })
        }
    },
    shop: {
        addItemInCart: {
            started: (payload: { itemId: string, quantity: number }) =>
                <AddItemInCartStartedAction>({ type: "SHOP_ADD_ITEM_IN_CART_STARTED", payload }),
            succeed: (payload: { itemId: string, quantity: number }) =>
                <AddItemInCartSucceedAction>({ type: "SHOP_ADD_ITEM_IN_CART_SUCCEED", payload }),
            failed: (error: any) =>
                <AddItemInCartFailedAction>({ type: "SHOP_ADD_ITEM_IN_CART_FAILED", error })
        },
        removeItemFromCart: {
            started: (payload: { itemId: string, quantity: number }) =>
                <RemoveItemFromCartStartedAction>({ type: "SHOP_REMOVE_ITEM_FROM_CART_STARTED", payload }),
            succeed: (payload: { itemId: string, quantity: number }) =>
                <RemoveItemFromCartSucceedAction>({ type: "SHOP_REMOVE_ITEM_FROM_CART_SUCCEED", payload }),
            failed: (error: any) =>
                <RemoveItemFromCartFailedAction>({ type: "SHOP_REMOVE_ITEM_FROM_CART_FAILED", error })
        },
        order: {
            started: () => <OrderStartedAction>({ type: "SHOP_ORDER_STARTED" }),
            succeed: () => <OrderSucceedAction>({ type: "SHOP_ORDER_SUCCEED" }),
            failed: (error: any) => <OrderFailedAction>({ type: "SHOP_ORDER_FAILED", error })
        },
        resetCart: {
            started: () => <ResetCartStartedAction>({ type: "SHOP_RESET_CART_STARTED" }),
            succeed: () => <ResetCartSucceedAction>({ type: "SHOP_RESET_CART_SUCCEED" }),
            failed: (error: any) => <ResetCartFailedAction>({ type: "SHOP_RESET_CART_FAILED", error })
        },
        upsert: (itemAndQuantity: { quantity: number, itemId: string }) =>
            <UpsertCartAction>({ type: "SHOP_UPSERT", itemAndQuantity })
    },
    delivery: {
        validate: {
            started: (payload: any) => <ValidateOrderStartedAction>({ type: "ORDER_VALIDATE_STARTED", payload }),
            succeed: () => <ValidateOrderSucceedAction>({ type: "ORDER_VALIDATE_SUCCEED" }),
            failed: (error: any) => <ValidateOrderFailedAction>({ type: "ORDER_VALIDATE_FAILED", error })
        },
        cancel: {
            started: (payload: any) => <CancelOrderStartedAction>({ type: "ORDER_CANCEL_STARTED", payload }),
            succeed: () => <CancelOrderSucceedAction>({ type: "ORDER_CANCEL_SUCCEED" }),
            failed: (error: any) => <CancelOrderFailedAction>({ type: "ORDER_CANCEL_FAILED", error })
        },
        upsert: (order: Order) => <UpsertOrderAction>({ type: "ORDER_UPSERT", order })
    },
    inventory: {
        createItem: {
            started: (payload: any) => <CreateItemStartedAction>({ type: "INVENTORY_CREATE_ITEM_STARTED", payload }),
            succeed: () => <CreateItemSucceedAction>({ type: "INVENTORY_CREATE_ITEM_SUCCEED" }),
            failed: (error: any) => <CreateItemFailedAction>({ type: "INVENTORY_CREATE_ITEM_FAILED", error })
        },
        updatePrice: {
            started: (payload: { itemId: string, newPrice: number }) =>
                <UpdatePriceStartedAction>({ type: "INVENTORY_UPDATE_PRICE_STARTED", payload }),
            succeed: () => <UpdatePriceSucceedAction>({ type: "INVENTORY_UPDATE_PRICE_SUCCEED" }),
            failed: (error: any) => <UpdatePriceFailedAction>({ type: "INVENTORY_UPDATE_PRICE_FAILED", error })
        },
        supply: {
            started: (payload: { itemId: string, quantity: number }) =>
                <SupplyItemStartedAction>({ type: "INVENTORY_SUPPLY_ITEM_STARTED", payload }),
            succeed: (itemId: string) => <SupplyItemSucceedAction>({ type: "INVENTORY_SUPPLY_ITEM_SUCCEED", itemId }),
            failed: (error: any) => <SupplyItemFailedAction>({ type: "INVENTORY_SUPPLY_ITEM_FAILED", error })
        },
        upsert: (item: Item) => <UpsertItemAction>({ type: "INVENTORY_UPSERT_ITEM", item })
    },
    events: {
        add: (event: Event) => <AddEventAction>({ type: "EVENT_ADD", event })
    }
};

const action$ = new Subject<Action>();

const dispatch = (action: Action) => {
    action$.next(action);
};

// state management
type FormPropertyState<T> = {
    value?: T,
    errorLevel?: "success" | "info" | "warning" | "danger" | undefined,
    message?: string | undefined
};

type State = {
    currentPage: "shop" | "orders" | "inventory" | "events",
    items: Item[],
    cart: Cart,
    orders: Order[],
    events: Event[],
    search: string,
    forms: {
        createItem: {
            name: FormPropertyState<string>,
            price: FormPropertyState<number>,
            initialQuantity: FormPropertyState<number>
        },
        updatePrice: { [itemId: string]: { price: FormPropertyState<number> } },
        supply: { [itemId: string]: { quantity: FormPropertyState<number> } }
    },
    previousState: any | undefined
};

const initialState: State = {
    currentPage: "shop",
    items: [],
    cart: {
        items: []
    },
    orders: [],
    events: [],
    search: "",
    forms: {
        createItem: {
            name: {
                value: ""
            },
            price: {
                value: 0,
                errorLevel: "success"
            },
            initialQuantity: {
                value: 0,
                errorLevel: "success"
            }
        },
        updatePrice: {},
        supply: {}
    },
    previousState: undefined
};

const reduce = (state: State, action: Action): State => {
    if (action.type === "APP_CHANGE_PAGE") {
        return {
            ...state,
            currentPage: action.pageName,
            search: ""
        };
    }
    if (action.type === "APP_UPDATE_SEARCH") {
        return {
            ...state,
            search: action.search
        };
    }
    if (action.type === "APP_LOAD_SUCCEED") {
        const updatePriceForms: { [itemId: string]: { price: FormPropertyState<number> } } = {};
        const supplyForms: { [itemId: string]: { quantity: FormPropertyState<number> } } = {};

        action.items.forEach(item => {
            updatePriceForms[item.id] = {
                price: {
                    value: item.price
                }
            };

            supplyForms[item.id] = {
                quantity: {
                    value: 0
                }
            };
        });

        return {
            ...state,
            items: action.items,
            cart: action.cart,
            orders: action.orders,
            events: action.events,
            forms: {
                ...state.forms,
                updatePrice: updatePriceForms,
                supply: supplyForms
            }
        };
    }
    if (action.type === "APP_UPDATE_FORM_ACTION") {
        const getUpdatedProperty = (property: string, newValue: any): FormPropertyState<any> => {
            if (property === "name") {
                if (state.items.some(item => item.name === newValue)) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "An item with the same name already exist"
                    };
                }

                if (!newValue) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "An item should have a name"
                    };
                }

                return {
                    value: newValue,
                    errorLevel: "success"
                };
            }
            if (property === "price") {
                if (!newValue) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "An item should have a price"
                    };
                }

                if (isNaN(newValue)) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "Should be a number"
                    };
                }

                if (parseFloat(newValue) < 0) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "Should be a positive number"
                    };
                }

                return {
                    value: parseFloat(newValue).toLocaleString('fr-FR'),
                    errorLevel: "success"
                };
            }
            if (property === "initialQuantity") {
                if (!newValue) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "An item should have a quantity"
                    };
                }

                if (isNaN(newValue)) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "Should be a number"
                    };
                }

                if (parseInt(newValue) < 0) {
                    return {
                        value: undefined,
                        errorLevel: "danger",
                        message: "Should be a positive number"
                    };
                }

                return {
                    value: parseInt(newValue).toLocaleString('fr-FR'),
                    errorLevel: "success"
                };
            }
        };

        if (action.formName === "createItem") {
            return {
                ...state,
                forms: {
                    ...state.forms,
                    [action.formName]: {
                        ...state.forms[action.formName],
                        [action.property]: getUpdatedProperty(action.property, action.value)
                    }
                }
            };
        }

        if (action.formName === "updatePrice") {
            const updatePriceForms: { [itemId: string]: { price: FormPropertyState<number> } } = {};

            state.items.forEach(item => {
                if (item.id === action.extendedProps.itemId) {
                    updatePriceForms[item.id] = {
                        price: {
                            value: action.value
                        }
                    };
                } else {
                    updatePriceForms[item.id] = state.forms.updatePrice[item.id];
                }
            });

            return {
                ...state,
                forms: {
                    ...state.forms,
                    updatePrice: updatePriceForms
                }
            };
        }

        if (action.formName === "supply") {
            const supplyForms: { [itemId: string]: { quantity: FormPropertyState<number> } } = {};

            state.items.forEach(item => {
                if (item.id === action.extendedProps.itemId) {
                    supplyForms[item.id] = {
                        quantity: {
                            value: action.value
                        }
                    };
                } else {
                    supplyForms[item.id] = state.forms.supply[item.id];
                }
            });

            return {
                ...state,
                forms: {
                    ...state.forms,
                    supply: supplyForms
                }
            };
        }
    }
    if (action.type === "APP_CLEAR_FORM_ACTION") {
        return {
            ...state,
            forms: {
                ...state.forms,
                [action.formName]: {
                    ...initialState.forms[action.formName]
                }
            }
        };
    }
    if (action.type === "SHOP_RESET_CART_SUCCEED") {
        return {
            ...state,
            cart: { ...initialState.cart }
        };
    }
    if (action.type === "SHOP_ORDER_SUCCEED") {
        return {
            ...state,
            cart: { ...initialState.cart }
        };
    }
    if (action.type === "SHOP_UPSERT") {
        const isNew = state.cart.items.filter(i => i.itemId === action.itemAndQuantity.itemId).length <= 0;
        const toRemove = action.itemAndQuantity.quantity === 0;

        if (toRemove) {
            return {
                ...state,
                cart: {
                    ...state.cart,
                    items: state.cart.items.filter(i => i.itemId !== action.itemAndQuantity.itemId)
                }
            };
        } else if (isNew) {
            return {
                ...state,
                cart: {
                    ...state.cart,
                    items: [...state.cart.items, action.itemAndQuantity]
                }
            };
        } else {
            return {
                ...state,
                cart: {
                    ...state.cart,
                    items: state.cart.items.map(i => {
                        if (i.itemId === action.itemAndQuantity.itemId) {
                            return action.itemAndQuantity;
                        }
                        return i;
                    })
                }
            };
        }
    }
    if (action.type === "ORDER_UPSERT") {
        const isNew = state.orders.filter(order => order.id === action.order.id).length <= 0;

        if (isNew) {
            return {
                ...state,
                orders: [...state.orders, action.order]
            };
        } else {
            return {
                ...state,
                orders: state.orders.map(order => {
                    if (order.id === action.order.id) {
                        return action.order;
                    }
                    return order;
                })
            };
        }
    }
    if (action.type === "INVENTORY_CREATE_ITEM_SUCCEED") {
        return {
            ...state,
            forms: {
                ...state.forms,
                createItem: initialState.forms.createItem
            }
        };
    }
    if (action.type === "INVENTORY_SUPPLY_ITEM_SUCCEED") {
        const supplyForms: { [itemId: string]: { quantity: FormPropertyState<number> } } = {};

        state.items.forEach(item => {
            if (item.id === action.itemId) {
                supplyForms[item.id] = {
                    quantity: {
                        value: 0
                    }
                };
            } else {
                supplyForms[item.id] = state.forms.supply[item.id];
            }
        });

        return {
            ...state,
            forms: {
                ...state.forms,
                supply: supplyForms
            }
        };
    }
    if (action.type === "INVENTORY_UPSERT_ITEM") {
        const isNew = state.items.filter(item => item.id === action.item.id).length <= 0;

        if (isNew) {
            return {
                ...state,
                items: [...state.items, action.item]
            };
        } else {
            return {
                ...state,
                items: state.items.map(item => {
                    if (item.id === action.item.id) {
                        return action.item;
                    }
                    return item;
                })
            };
        }
    }
    if (action.type === "EVENT_ADD") {
        return {
            ...state,
            events: [...state.events, action.event]
        }
    }
    return state;
};

const state$ = action$.pipe<State>(
    startWith(initialState),
    scan(reduce)
);

// update UI
const pageChange$ = state$.pipe(
    map(state => state.currentPage),
    distinctUntilChanged()
);

const cartChange$ = state$.pipe(
    map(state => state.cart),
    distinctUntilChanged()
);

const itemsChange$ = state$.pipe(
    map(state => state.items),
    distinctUntilChanged()
);

const ordersChange$ = state$.pipe(
    map(state => state.orders),
    distinctUntilChanged()
);

const eventsChange$ = state$.pipe(
    map(state => state.events),
    distinctUntilChanged()
);

const createItemFormChange$ = state$.pipe(
    map(state => state.forms.createItem),
    distinctUntilChanged()
);

const updatePriceFormsChanged$ = state$.pipe(
    map(state => state.forms.updatePrice),
    distinctUntilChanged()
);

const supplyFormsChanged$ = state$.pipe(
    map(state => state.forms.supply),
    distinctUntilChanged()
);

const searchChange$ = state$.pipe(
    map(state => state.search),
    distinctUntilChanged()
);

const itemsSearched$ = combineLatest(itemsChange$, searchChange$).pipe(
    map(([items, search]) => {
        const searchLower = search.toLowerCase();
        return items.filter(item => item.name.toLowerCase().includes(searchLower));
    })
);

const searchInput$ = new Subject<string>();
searchInput$.pipe(
    debounceTime(150),
    distinctUntilChanged(),
    map(actionsCreator.app.updateSearch)
).subscribe(dispatch);

const navbar$ = pageChange$.pipe(
    map(currentPage => {
        const getNavBarItemStyle = (navBarItemType: string) => {
            if (navBarItemType === currentPage) {
                return "border-bottom: 2px solid black;";
            }
            return "";
        };

        const shopLink = h("a", {
            className: "navbar-item",
            style: getNavBarItemStyle("shop"),
            onclick: () => dispatch(actionsCreator.app.changePage("shop"))
        }, ["Shop"]);
        const ordersLink = h("a", {
            className: "navbar-item",
            style: getNavBarItemStyle("orders"),
            onclick: () => dispatch(actionsCreator.app.changePage("orders"))
        }, ["Orders"]);
        const inventoryLink = h("a", {
            className: "navbar-item",
            style: getNavBarItemStyle("inventory"),
            onclick: () => dispatch(actionsCreator.app.changePage("inventory"))
        }, ["Inventory"]);
        const eventsLink = h("a", {
            className: "navbar-item",
            style: getNavBarItemStyle("events"),
            onclick: () => dispatch(actionsCreator.app.changePage("events"))
        }, ["Events"]);

        return h("nav", {
            className: "navbar is-fixed-top is-light has-shadow",
            role: "navigation",
            "aria-label": "main navigation"
        }, [
                h("div", {
                    className: "navbar-brand",
                    style: "padding: 18px 20px 15px 15px; color: grey; font-size: 12px;"
                }, ["Simple Event Sourcing - Shopping Example app"]),
                h("div", { className: "navbar-menu" }, [
                    h("div", { className: "navbar-start" }, [
                        shopLink,
                        ordersLink,
                        inventoryLink,
                        eventsLink
                    ])
                ])
            ]);
    })
);

const searchComponent$ = searchChange$.pipe(
    map(search => {
        const searchInput = h("input", {
            className: "input",
            type: "text",
            placeholder: "search item",
            defaultValue: search,
            onkeyup: (e: KeyboardEvent) => searchInput$.next((<HTMLInputElement>(e.target)).value)
        }, []);

        return h("div", { className: "panel-block" }, [
            h("div", { className: "control has-icons-left" }, [
                searchInput,
                h("span", { className: "icon is-left" }, [
                    h("i", { className: "fas fa-search", "aria-hidden": true }, [])
                ])
            ])
        ]);
    })
);

const addToCartComponent$ = combineLatest(searchComponent$, itemsSearched$, cartChange$).pipe(
    map(([searchComponent, items, cart]) => {
        const displayItemsLeft = (quantityLeftToAdd: number) => {
            if (quantityLeftToAdd > 1) {
                return quantityLeftToAdd + " items left";
            }
            if (quantityLeftToAdd === 1) {
                return quantityLeftToAdd + " item left";
            }
            return "No item left";
        };

        const top5Items = items
            .filter(item => item.remainingQuantity > 0)
            .slice(0, 5)
            .map(item => {
                const itemInCart = cart.items.filter(i => i.itemId === item.id)[0];
                const quantityLeftToAdd =
                    itemInCart ? (item.remainingQuantity - itemInCart.quantity) : item.remainingQuantity;

                return h("div", { key: item.id.toString(), className: "panel-block", style: { display: 'flex' } }, [
                    h("div", { style: { display: 'grid' } }, [
                        h("button", {
                            className: "button is-rounded is-primary is-outlined",
                            style: { width: '30px', height: '30px', marginBottom: '2px', padding: 0 },
                            onclick: () => dispatch(actionsCreator.shop.addItemInCart.started({ itemId: item.id, quantity: 1 }))
                        }, ["+"]),
                        h("button", {
                            disabled: !itemInCart,
                            className: "button is-rounded is-dark is-outlined",
                            style: { width: '30px', height: '30px', marginTop: '2px', padding: 0 },
                            onclick: () => dispatch(actionsCreator.shop.removeItemFromCart.started({ itemId: item.id, quantity: 1 }))
                        }, ["-"])
                    ]),
                    h("div", { style: { display: 'block', marginLeft: '15px' } }, [
                        h("div", {}, [item.name]),
                        h("div", { style: { fontSize: '12px' } }, [
                            displayItemsLeft(quantityLeftToAdd)
                        ])
                    ])
                ])
            });

        return h("div", { className: "panel column" }, [
            h("div", { className: "panel-heading" }, ["list of items"]),
            searchComponent,
            ...top5Items // list of items searched (to add in the cart)
        ]);
    })
);

const cartComponent$ = combineLatest(cartChange$, itemsChange$).pipe(
    map(([cart, items]) => {
        const cartItems = cart.items
            .map(cartItem => {
                return {
                    cartItem,
                    item: items.filter(i => i.id === cartItem.itemId)[0]
                };
            })
            .filter(x => !!x.item)
            .map(({ cartItem, item }) => {
                return h("div", {
                    key: cartItem.itemId.toString(),
                    className: "panel-block",
                    style: { justifyContent: 'space-between' }
                }, [
                        h("div", { style: { display: 'block' } }, [
                            h("div", {}, [item.name]),
                            h("div", { style: { fontSize: '12px' } }, ["x" + cartItem.quantity]),
                        ]),
                        h("div", { className: "field is-grouped is-grouped-right" }, [
                            h("div", { className: "control" }, [
                                h("button", {
                                    disabled: false,
                                    className: "button is-rounded is-danger is-outlined",
                                    style: { fontSize: '12px' },
                                    onclick: () => dispatch(actionsCreator.shop.removeItemFromCart.started({ itemId: cartItem.itemId, quantity: cartItem.quantity }))
                                }, ["remove"])
                            ])
                        ])
                    ]);
            });

        return h("div", { className: "panel column is-one-third" }, [
            h("div", { className: "panel-heading" }, ["my cart"]),
            ...cartItems, // list of items in cart
            h("div", { className: "panel-block", style: { display: "block" } }, [
                h("div", { className: "field is-grouped is-grouped-right" }, [
                    h("div", { className: "control" }, [
                        h("button", {
                            disabled: cart.items.length === 0,
                            className: "button is-rounded is-primary is-outlined is-fullwidth",
                            onclick: () => dispatch(actionsCreator.shop.order.started())
                        }, ["order"])
                    ]),
                    h("div", { className: "control" }, [
                        h("button", {
                            disabled: cart.items.length === 0,
                            className: "button is-rounded is-danger is-outlined is-fullwidth",
                            onclick: () => dispatch(actionsCreator.shop.resetCart.started())
                        }, ["reset"])
                    ])
                ])
            ])
        ]);
    })
);

const shopComponent$ = combineLatest(addToCartComponent$, cartComponent$).pipe(
    map(([addToCartComponent, cartComponent]) => {
        return h("div", { className: "container" }, [
            h("div", { className: "columns" }, [
                addToCartComponent,
                cartComponent
            ])
        ]);
    })
);

const ordersComponent$ = combineLatest(ordersChange$, itemsChange$).pipe(
    map(([orders, items]) => {
        if (orders.length === 0) {
            return h("section", { className: "hero" }, [
                h("div", { className: "hero-body" }, [
                    h("div", { className: "container" }, [
                        h("div", { className: "title" }, ["No order"]),
                        h("div", { className: "subtitle" }, ["There is currently no order in the system..."])
                    ])
                ])
            ]);
        }

        return h("section", {}, [
            h("div", { className: "container" }, [
                orders.map(order => {
                    const totalPrice = order.items
                        .map(item => item.price * item.quantity)
                        .reduce((a, b) => a + b, 0);

                    const canOrder = order.items.every(itemOrdered => {
                        const item = items.filter(i => i.id === itemOrdered.itemId)[0];
                        if (item) {
                            const missingQuantity = itemOrdered.quantity - item.remainingQuantity;
                            return missingQuantity <= 0;
                        }

                        return false;
                    });

                    return h("div", { className: "card", key: order.id.toString(), style: { marginBottom: '10px' } }, [
                        h("div", { className: "card-header" }, [
                            h("div", { className: "card-header-title" }, [
                                "Order #" + order.number
                            ])
                        ]),
                        h("div", {
                            className: "card-content"
                        }, [
                                h("div", {}, [
                                    order.items.map(itemOrdered => {
                                        const item = items.filter(i => i.id === itemOrdered.itemId)[0];

                                        if (item) {
                                            const missingQuantity = itemOrdered.quantity - item.remainingQuantity;

                                            return h("div", {}, [
                                                h("span", {}, [item.name]),
                                                h("span", { style: { fontSize: '12px' } }, [" x" + itemOrdered.quantity]),
                                                missingQuantity > 0 ?
                                                    h("span", { style: { fontSize: '12px' }, className: "has-text-danger" }, [" (" + missingQuantity + " missing)"])
                                                    : null
                                            ]);
                                        }
                                        return h("div", []);
                                    })
                                ]),
                                h("div", { style: { marginTop: '10px', fontSize: '12px' } }, [
                                    "Total: " + totalPrice + " €"
                                ]),
                                h("div", { style: { marginTop: '10px' } }, [
                                    (!order.isConfirmed && !order.isCanceled) ? h("button", {
                                        className: "button is-rounded is-primary is-outlined",
                                        style: { fontSize: '12px' },
                                        disabled: !canOrder,
                                        onclick: () => dispatch(actionsCreator.delivery.validate.started({
                                            orderId: order.id
                                        }))
                                    }, ["validate order"]) : null,
                                    (!order.isConfirmed && !order.isCanceled) ? h("button", {
                                        className: "button is-rounded is-danger is-outlined",
                                        style: { fontSize: '12px', marginLeft: '10px' },
                                        onclick: () => dispatch(actionsCreator.delivery.cancel.started({
                                            orderId: order.id
                                        }))
                                    }, ["cancel order"]) : null,
                                    order.isConfirmed ? h("div", {}, ["order confirmed"]) : null,
                                    order.isCanceled ? h("div", {}, ["order canceled"]) : null
                                ])
                            ])
                    ]);
                })
            ])
        ]);
    })
);

const createItemComponent$ = createItemFormChange$.pipe(
    map(createItemForm => {
        const updateForm = (property: string, value: any) =>
            dispatch(actionsCreator.app.updateForm("createItem", property, value));

        const isValidForm = createItemForm.name.errorLevel === "success" &&
            createItemForm.price.errorLevel === "success" &&
            createItemForm.initialQuantity.errorLevel === "success";

        const withErrorLevelClassname = (className: string, errorLevel: string | undefined) => {
            if (errorLevel) {
                return className + ' ' + 'is-' + errorLevel;
            } else {
                return className;
            }
        }

        return h("section", { className: "card" }, [
            h("div", { className: "card-content" }, [
                h("label", { className: "label", style: { fontSize: '11px', color: 'gray' } }, ["Name"]),
                h("div", { className: "field" }, [
                    h("div", { className: "control" }, [
                        h("input", {
                            className: withErrorLevelClassname("input", createItemForm.name.errorLevel),
                            type: "text",
                            placeholder: "Name of the item",
                            value: createItemForm.name.value,
                            onkeyup: (e: KeyboardEvent) => updateForm("name", (<HTMLInputElement>(e.target)).value)
                        }, [])
                    ]),
                    h("div",
                        { className: withErrorLevelClassname("help", createItemForm.name.errorLevel) },
                        [createItemForm.name.message]
                    )
                ]),
                h("label", { className: "label", style: { fontSize: '11px', color: 'gray' } }, ["Price"]),
                h("div", { className: "field has-addons has-addons-right", style: { flexWrap: 'wrap' } }, [
                    h("div", { className: "control is-expanded" }, [
                        h("input", {
                            className: withErrorLevelClassname("input", createItemForm.price.errorLevel),
                            type: "text",
                            placeholder: "Unit cost of the item",
                            value: createItemForm.price.value,
                            onkeyup: (e: KeyboardEvent) => updateForm("price", (<HTMLInputElement>(e.target)).value)
                        }, [])
                    ]),
                    h("div", { className: "control" }, [
                        h("a", { className: "button is-static" }, ["€"])
                    ]),
                    h("div",
                        { className: withErrorLevelClassname("help", createItemForm.price.errorLevel), style: { width: '100%' } },
                        [createItemForm.price.message]
                    )
                ]),
                h("label", { className: "label", style: { fontSize: '11px', color: 'gray' } }, ["Initial quantity"]),
                h("div", { className: "field" }, [
                    h("div", { className: "control" }, [
                        h("input", {
                            className: withErrorLevelClassname("input", createItemForm.initialQuantity.errorLevel),
                            type: "text",
                            placeholder: "Quantity of item",
                            value: createItemForm.initialQuantity.value,
                            onkeyup: (e: KeyboardEvent) => updateForm("initialQuantity", (<HTMLInputElement>(e.target)).value)
                        }, [])
                    ]),
                    h("div",
                        { className: withErrorLevelClassname("help", createItemForm.initialQuantity.errorLevel) },
                        [createItemForm.initialQuantity.message]
                    )
                ]),
                h("div", { className: "control" }, [
                    h("button", {
                        disabled: !isValidForm,
                        className: "button is-rounded is-primary is-outlined",
                        onclick: () => dispatch(actionsCreator.inventory.createItem.started({
                            name: createItemForm.name.value,
                            price: createItemForm.price.value,
                            initialQuantity: createItemForm.initialQuantity.value,
                        }))
                    }, ["Create new item"])
                ])
            ])
        ]);
    })
);

const itemListComponent$ = combineLatest(itemsChange$, updatePriceFormsChanged$, supplyFormsChanged$).pipe(
    map(([items, updatePriceForms, supplyForms]) => {
        if (items.length === 0) {
            return h("section", { className: "hero" }, [
                h("div", { className: "hero-body" }, [
                    h("div", { className: "container" }, [
                        h("div", { className: "title" }, ["No items left"]),
                        h("div", { className: "subtitle" }, ["There is no more items in the system..."])
                    ])
                ])
            ]);
        }

        return h("section", { className: "section" }, [
            h("div", { className: "container" },
                items.map(item => {
                    const supplyForm = supplyForms[item.id];
                    const updatePriceForm = updatePriceForms[item.id];

                    return h("div", { className: "card", key: item.id.toString(), style: { marginBottom: '10px' } }, [
                        h("div", { className: "card-header" }, [
                            h("div", { className: "card-header-title" }, [
                                item.name
                            ])
                        ]),
                        h("div", {
                            className: "card-content"
                        }, [
                                h("div", { className: "columns" }, [
                                    h("div", { className: "column" }, [
                                        h("div", [
                                            item.price + " €"
                                        ]),
                                        h("div", [
                                            "Stock: " + item.remainingQuantity
                                        ])
                                    ]),
                                    h("div", {
                                        className: "column has-background-white-bis",
                                        style: { display: "flex", alignItems: "center" }
                                    }, [
                                            h("input", {
                                                className: "input",
                                                style: { width: '200px' },
                                                type: "text",
                                                placeholder: "New price",
                                                value: updatePriceForm ? updatePriceForm.price.value : "",
                                                onkeyup: (e: KeyboardEvent) => dispatch(actionsCreator.app.updateForm("updatePrice", "price", (<HTMLInputElement>(e.target)).value, { itemId: item.id }))
                                            }, []),
                                            h("button", {
                                                disabled: !(updatePriceForm && !isNaN(updatePriceForm.price.value) && updatePriceForm.price.value != item.price),
                                                className: "button is-rounded is-default is-outlined",
                                                style: { fontSize: '12px', marginLeft: '10px' },
                                                onclick: () => dispatch(actionsCreator.inventory.updatePrice.started({
                                                    itemId: item.id,
                                                    newPrice: updatePriceForm.price.value
                                                }))
                                            }, ["update price"])
                                        ]),
                                    h("div", {
                                        className: "column has-background-white-ter",
                                        style: { display: "flex", alignItems: "center" }
                                    }, [
                                            h("input", {
                                                className: "input",
                                                style: { width: '150px' },
                                                type: "text",
                                                placeholder: "Supply quantity",
                                                value: supplyForm ? supplyForm.quantity.value : "",
                                                onkeyup: (e: KeyboardEvent) => dispatch(actionsCreator.app.updateForm("supply", "quantity", (<HTMLInputElement>(e.target)).value, { itemId: item.id }))
                                            }, []),
                                            h("button", {
                                                disabled: !(supplyForm && !isNaN(supplyForm.quantity.value) && supplyForm.quantity.value > 0),
                                                className: "button is-rounded is-default is-outlined",
                                                style: { fontSize: '12px', marginLeft: '10px' },
                                                onclick: () => dispatch(actionsCreator.inventory.supply.started({
                                                    itemId: item.id,
                                                    quantity: supplyForm.quantity.value
                                                }))
                                            }, ["supply"])
                                        ])
                                ])
                            ])
                    ]);
                })
            )
        ]);
    })
);

const inventoryComponent$ = combineLatest(createItemComponent$, itemListComponent$).pipe(
    map(([createItemComponent, itemListComponent]) => {
        return h("div", { classname: "container" }, [
            h("div", { className: "columns is-centered" }, [
                h("div", { className: "column is-half" }, [createItemComponent])
            ]),
            itemListComponent
        ]);
    })
);

const eventsComponent$ = eventsChange$.pipe(
    map(events => {
        if (events.length === 0) {
            return h("section", { className: "hero" }, [
                h("div", { className: "hero-body" }, [
                    h("div", { className: "container" }, [
                        h("div", { className: "title" }, ["No event"]),
                        h("div", { className: "subtitle" }, ["There is currently no event created..."])
                    ])
                ])
            ]);
        }

        return h("section", {}, [
            h("div", { className: "container" }, [
                ...events.sort((a, b) => b.number - a.number).map(event => {
                    return h("div", { className: "card", key: event.id.toString(), style: { marginBottom: '10px' } }, [
                        h("div", { className: "card-header" }, [
                            h("div", { className: "card-header-title" }, [
                                "Event #" + event.number + " - " + event.eventName
                            ])
                        ]),
                        h("div", { className: "card-content" }, [
                            h("div", [
                                JSON.stringify(event.data, null, 4)
                            ])
                        ])
                    ]);
                })
            ])
        ]);
    })
);

const body$ = combineLatest(pageChange$, shopComponent$, inventoryComponent$, ordersComponent$, eventsComponent$).pipe(
    map(([currentPage, shopComponent, inventoryComponent, ordersComponent, eventsComponent]) => {
        if (currentPage === "shop") {
            return shopComponent;
        }
        if (currentPage === "inventory") {
            return inventoryComponent;
        }
        if (currentPage === "orders") {
            return ordersComponent;
        }
        if (currentPage === "events") {
            return eventsComponent;
        }
    })
);

const vdom$ = combineLatest(navbar$, body$).pipe(
    map(([navbar, body]) => {
        return h("div", {}, [
            navbar,
            h("section", { className: "section" }, [body])
        ]);
    })
);

// API calls (through epics)
const ofType = (type: string) => {
    return filter<Action>(action => action.type === type);
};

const httpHeaders = { 
    "Content-Type": "application/json" 
};

const loadAppEpic$ = action$.pipe(
    ofType("APP_LOAD_STARTED"),
    mergeMap(_ =>
        forkJoin([
            ajax.getJSON<Item[]>(inventoryService.url + "api/all"),
            ajax.getJSON<Cart>(shopService.url + "api/cart"),
            ajax.getJSON<Order[]>(deliveryService.url + "api/all"),
            ajax.getJSON<Event[]>(eventHistoryService.url + "api/events")
        ]).pipe(
            map(([items, cart, orders, events]) => {
                return actionsCreator.app.load.succeed(items, cart, orders, events);
            }),
            catchError(error => of(actionsCreator.app.load.failed(error)))
        )
    )
);

const addItemInCartEpic$ = action$.pipe(
    ofType("SHOP_ADD_ITEM_IN_CART_STARTED"),
    mergeMap((action: AddItemInCartStartedAction) =>
        ajax.post(shopService.url + "api/cart/addItem", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.shop.addItemInCart.succeed(action.payload)),
            catchError(error => of(actionsCreator.shop.addItemInCart.failed(error)))
        )
    )
);

const removeItemFromCartEpic$ = action$.pipe(
    ofType("SHOP_REMOVE_ITEM_FROM_CART_STARTED"),
    mergeMap((action: RemoveItemFromCartStartedAction) =>
        ajax.post(shopService.url + "api/cart/removeItem", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.shop.removeItemFromCart.succeed(action.payload)),
            catchError(error => of(actionsCreator.shop.removeItemFromCart.failed(error)))
        )
    )
);

const resetCartEpic$ = action$.pipe(
    ofType("SHOP_RESET_CART_STARTED"),
    mergeMap(_ =>
        ajax.post(shopService.url + "api/cart/reset", {}, httpHeaders).pipe(
            map(_ => actionsCreator.shop.resetCart.succeed()),
            catchError(error => of(actionsCreator.shop.resetCart.failed(error)))
        )
    )
);

const orderEpic$ = action$.pipe(
    ofType("SHOP_ORDER_STARTED"),
    mergeMap(_ =>
        ajax.post(shopService.url + "api/order", {}, httpHeaders).pipe(
            map(_ => actionsCreator.shop.order.succeed()),
            catchError(error => of(actionsCreator.shop.order.failed(error)))
        )
    )
);

const validateOrderEpic$ = action$.pipe(
    ofType("ORDER_VALIDATE_STARTED"),
    mergeMap((action: ValidateOrderStartedAction) =>
        ajax.post(deliveryService.url + "api/validate", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.delivery.validate.succeed()),
            catchError(error => of(actionsCreator.delivery.validate.failed(error)))
        )
    )
);

const cancelOrderEpic$ = action$.pipe(
    ofType("ORDER_CANCEL_STARTED"),
    mergeMap((action: CancelOrderStartedAction) =>
        ajax.post(deliveryService.url + "api/cancel", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.delivery.cancel.succeed()),
            catchError(error => of(actionsCreator.delivery.cancel.failed(error)))
        )
    )
);

const createItemEpic$ = action$.pipe(
    ofType("INVENTORY_CREATE_ITEM_STARTED"),
    mergeMap((action: CreateItemStartedAction) =>
        ajax.post(inventoryService.url + "api/create", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.inventory.createItem.succeed()),
            catchError(error => of(actionsCreator.inventory.createItem.failed(error)))
        )
    )
);
const createItemSucceedEpic$ = action$.pipe(
    ofType("INVENTORY_CREATE_ITEM_SUCCEED"),
    map(_ => actionsCreator.app.clearForm("createItem"))
);

const updateItemPriceEpic$ = action$.pipe(
    ofType("INVENTORY_UPDATE_PRICE_STARTED"),
    mergeMap((action: UpdatePriceStartedAction) =>
        ajax.post(inventoryService.url + "api/updatePrice", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.inventory.updatePrice.succeed()),
            catchError(error => of(actionsCreator.inventory.updatePrice.failed(error)))
        )
    )
);

const supplyItemEpic$ = action$.pipe(
    ofType("INVENTORY_SUPPLY_ITEM_STARTED"),
    mergeMap((action: SupplyItemStartedAction) =>
        ajax.post(inventoryService.url + "api/supply", action.payload, httpHeaders).pipe(
            map(_ => actionsCreator.inventory.supply.succeed(action.payload.itemId)),
            catchError(error => of(actionsCreator.inventory.supply.failed(error)))
        )
    )
);

const epics: Observable<Action>[] = [
    loadAppEpic$,
    addItemInCartEpic$,
    removeItemFromCartEpic$,
    resetCartEpic$,
    orderEpic$,
    validateOrderEpic$,
    cancelOrderEpic$,
    createItemEpic$,
    createItemSucceedEpic$,
    updateItemPriceEpic$,
    supplyItemEpic$
];

merge(...epics).subscribe(dispatch);

// observe websockets (via signalr)
const eventConnection = new HubConnectionBuilder().withUrl(eventHistoryService.url + "event").build();
eventConnection.on("Sync", event => {
    dispatch(actionsCreator.events.add(event));
});
eventConnection.start().catch(error => {
    return console.error(error.toString());
});

const cartConnection = new HubConnectionBuilder().withUrl(shopService.url + "cart").build();
cartConnection.on("Sync", itemAndQuantity => {
    dispatch(actionsCreator.shop.upsert(itemAndQuantity));
});
cartConnection.start().catch(error => {
    return console.error(error.toString());
});

const orderConnection = new HubConnectionBuilder().withUrl(deliveryService.url + "order").build();
orderConnection.on("Sync", order => {
    dispatch(actionsCreator.delivery.upsert(order));
});
orderConnection.start().catch(error => {
    return console.error(error.toString());
});

const itemConnection = new HubConnectionBuilder().withUrl(inventoryService.url + "item").build();
itemConnection.on("Sync", item => {
    dispatch(actionsCreator.inventory.upsert(item));
});
itemConnection.start().catch(error => {
    return console.error(error.toString());
});

// start rendering app
while (rootNode.hasChildNodes()) {
    rootNode.removeChild(rootNode.firstChild);
}

vdom$.pipe(
    debounceTime(15),
    pairwise()
).subscribe(([oldTree, newTree]) => {
    const patches = diff(oldTree, newTree);
    patch(rootNode, patches);
});

vdom$.pipe(first()).subscribe(newTree => {
    const patches = diff(h("div", []), newTree);
    patch(rootNode, patches);
});

// start app (load data)
dispatch(actionsCreator.app.load.started());