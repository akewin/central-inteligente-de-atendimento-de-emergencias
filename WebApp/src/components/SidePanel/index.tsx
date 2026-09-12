import {
  Box,
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Tooltip,
} from "@mui/material";
import { useLocation } from "react-router-dom";
import AuthUserDisplay from "../AuthUserDisplay";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faSignOutAlt } from "@fortawesome/free-solid-svg-icons";
import { useAuth, usePermissions } from "../../hooks";

interface SidePanelProps {
  open: boolean;
  onNavigate: (route: string) => void;
}

const DRAWER_OPEN = 260;
const DRAWER_CLOSED = 52;

export default function SidePanel({ open, onNavigate }: SidePanelProps) {
  const { authUser, handleLogout } = useAuth();
  const { getMenuItemsForUser } = usePermissions();
  const location = useLocation();

  const drawerWidth = open ? DRAWER_OPEN : DRAWER_CLOSED;

  const filteredMenu = getMenuItemsForUser(authUser);

  return (
    <Drawer
      variant="permanent"
      open={open}
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        whiteSpace: "nowrap",
        transition: (theme) =>
          theme.transitions.create("width", {
            easing: theme.transitions.easing.sharp,
            duration: theme.transitions.duration.enteringScreen,
          }),
        "& .MuiDrawer-paper": {
          width: drawerWidth,
          transition: (theme) =>
            theme.transitions.create("width", {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.enteringScreen,
            }),
          overflowX: "hidden",
        },
      }}
    >
      <Box sx={{ overflow: "hidden" }}>
        <AuthUserDisplay collapsed={!open} />

        <List>
          {filteredMenu.map((item) => {
            const isActive = location.pathname === item.route;

            return (
              <ListItem key={item.label} disablePadding>
                <Tooltip
                  title={item.label}
                  placement="right"
                  arrow
                  disableHoverListener={open}
                >
                  <ListItemButton
                    onClick={() => onNavigate(item.route)}
                    sx={{
                      backgroundColor: isActive
                        ? (theme) => `${theme.palette.primary.main}`
                        : "transparent",
                    }}
                  >
                    <ListItemIcon>
                      <FontAwesomeIcon icon={item.icon} />
                    </ListItemIcon>
                    <ListItemText
                      primary={item.label}
                      sx={{
                        opacity: open ? 1 : 0,
                        transition: "opacity 0.3s",
                        whiteSpace: "nowrap",
                      }}
                    />
                  </ListItemButton>
                </Tooltip>
              </ListItem>
            );
          })}

          <ListItem disablePadding>
            <Tooltip
              title="Sair"
              placement="right"
              arrow
              disableHoverListener={open}
            >
              <ListItemButton
                onClick={async () => {
                  await handleLogout();
                  onNavigate("/login");
                }}
              >
                <ListItemIcon>
                  <FontAwesomeIcon icon={faSignOutAlt} />
                </ListItemIcon>
                <ListItemText
                  primary="Sair"
                  sx={{
                    opacity: open ? 1 : 0,
                    transition: "opacity 0.3s",
                    whiteSpace: "nowrap",
                  }}
                />
              </ListItemButton>
            </Tooltip>
          </ListItem>
        </List>
      </Box>
    </Drawer>
  );
}
